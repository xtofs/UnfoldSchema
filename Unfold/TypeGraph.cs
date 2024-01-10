namespace Unfold;


// https://en.wikipedia.org/wiki/Adjacency_list
/// <summary>
/// A graph of structured types of a CSDL schema with edges that represent 
/// URL path segments to get from one type to the next,
/// represented as a adjecency list of fully qualified type names
/// https://en.wikipedia.org/wiki/Adjacency_list
/// </summary>
public record class TypeGraph(FrozenDictionary<string, List<Edge>> AdjencencyList)
{
    public const string ServiceRootNodeName = "$serviceRoot";

    public static TypeGraph FromModel(IEdmModel model, ILogger logger)
    {
        var adjacencyList = new Dictionary<string, List<Edge>>();

        AddContainer(model, logger, adjacencyList);


        foreach (var schemaElement in model.SchemaElements)
        {
            switch (schemaElement)
            {
                case IEdmStructuredType structuredType:
                    AddStructuredType(model, logger, adjacencyList, structuredType);
                    break;
                default:
                    break;
            }
        }

        return new TypeGraph(adjacencyList.ToFrozenDictionary());
    }

    private static void AddContainer(IEdmModel model, ILogger logger, Dictionary<string, List<Edge>> adjacencyList)
    {
        var edges = new List<Edge>();
        adjacencyList.Add(ServiceRootNodeName, edges);
        foreach (var schemaContainerElement in model.EntityContainer.Elements)
        {
            switch (schemaContainerElement)
            {
                case IEdmEntitySet entitySet:
                    {
                        var entityType = entitySet.EntityType();
                        var keys = entityType.Key().Select(property => property.Name);
                        if (keys.Count() != 1)
                        {
                            logger.LogWarning("composite key: {type} {keys}", entityType.FullName(), string.Join(", ", keys));
                        }
                        edges.Add(new Edge(entitySet.Name, entityType.FullTypeName(), true, string.Join(",", keys)));
                    }
                    break;
                case IEdmSingleton singleton:
                    {
                        var entityType = singleton.EntityType();
                        edges.Add(new Edge(singleton.Name, entityType.FullTypeName(), false));
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private static void AddStructuredType(IEdmModel model, ILogger logger, Dictionary<string, List<Edge>> adjacencyList, IEdmStructuredType structuredType)
    {
        if (adjacencyList.ContainsKey(structuredType.FullTypeName()))
        {
            return;
        }
        var edges = new List<Edge>();
        adjacencyList.Add(structuredType.FullTypeName(), edges);

        // properties
        foreach (var property in structuredType.Properties())
        {
            var propertyType = property.Type.Definition;
            var isMultiValued = false;
            if (propertyType is IEdmCollectionType collectionType)
            {
                propertyType = collectionType.ElementType.Definition;
                isMultiValued = true;
            }

            if (propertyType is IEdmEntityType propertyEntityType)
            {
                var keys = propertyEntityType.Key().Select(property => property.Name);
                if (propertyEntityType.Key().Count() != 1)
                {
                    logger.LogWarning("composite key: {type} {keys}", propertyEntityType.FullName(), string.Join(", ", keys));
                }
                if (propertyEntityType.Key().Any(k => k.Type.PrimitiveKind() != EdmPrimitiveTypeKind.String))
                {
                    logger.LogWarning("non string key: {type} {keys} {keyType}",
                    propertyEntityType.FullName(), string.Join(", ", keys), propertyEntityType.Key().First().Type.PrimitiveKind());
                }
                edges.Add(new Edge(property.Name, propertyEntityType.FullTypeName(), isMultiValued, keys.FirstOrDefault()));
            }
            else if (propertyType is IEdmComplexType complexType)
            {
                edges.Add(new Edge(property.Name, complexType.FullTypeName(), isMultiValued));
            }
        }

        // derived types
        foreach (var subtype in model.FindAllDerivedTypes(structuredType))
        {
            edges.Add(new Edge(subtype.FullTypeName(), subtype.FullTypeName(), false));
        }
    }

    public int NodeCount => AdjencencyList.Count;

    public int EdgeCount => AdjencencyList.Values.Sum(e => e.Count);

    public bool Validate(out IReadOnlyList<string> errors)
    {
        var result = new List<string>();
        foreach (var (type, properties) in this.AdjencencyList)
        {
            foreach (var edge in properties)
            {
                if (!AdjencencyList.ContainsKey(edge.Type))
                {
                    result.Append($"undefined property type {type}.{edge.Name} {edge.Type}");
                }
            }
        }
        errors = result;
        return result.Count != 0;
    }

    public void WriteTo(string path)
    {
        using var file = File.CreateText(path);
        WriteTo(file);
    }

    public void WriteTo(TextWriter writer)
    {
        foreach (var (type, properties) in this.AdjencencyList.OrderBy(kvp => kvp.Key))
        {
            writer.WriteLine($"[{type}]");
            foreach (var edge in properties)
            {
                if (edge.IsMultiValued)
                {
                    writer.WriteLine($"\t{edge.Name} -> {{{edge.Key}}} -> [{edge.Type}] ");
                }
                else
                {
                    writer.WriteLine($"\t{edge.Name} -> [{edge.Type}] ");
                }
            }
        }
    }

    public IEnumerable<ImmutableList<string>> Unfold(ILogger logger, int depth = 3)
    {
        if (depth <= 0)
        {
            yield break;
        }
        foreach (var edge in this.AdjencencyList[ServiceRootNodeName])
        {
            logger.LogInformation("processing {containerElementName}", edge.Name);
            var path = ImmutableList.Create(edge.Name);
            yield return path;
            if (edge.IsMultiValued)
            {
                path = path.Add($"{{{edge.Key}}}");
                yield return path;
            }
            foreach (var successor in Unfold(edge.Type, path, ImmutableHashSet.Create(edge.Type), depth))
            {
                yield return successor;
            }
        }
    }

    internal IEnumerable<ImmutableList<string>> Unfold(string type, ImmutableList<string> prefix, ImmutableHashSet<string> visited, int depth)
    {
        if (depth <= 0)
        {
            yield break;
        }
        foreach (var edge in this.AdjencencyList[type])
        {
            var path = prefix.Add(edge.Name);
            yield return path;
            if (edge.IsMultiValued)
            {
                path = path.Add($"{{{edge.Key}}}");
                depth -= 1;
                yield return path;
            }
            if (!visited.Contains(edge.Type))
            {
                foreach (var x in Unfold(edge.Type, path, visited.Add(edge.Type), depth))
                {
                    yield return x;
                }
            }
        }
    }
}


/// <summary>
/// the edges in the adjaceny list of a <see cref="TypeGraph"/>
/// </summary>
public record class Edge(string Name, string Type, bool IsMultiValued, string? Key = default)
{
}