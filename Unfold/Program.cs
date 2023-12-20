using Microsoft.Extensions.Logging;
using Serilog;
using Unfold;

class Program
{
    private static void Main()
    {
        var logger = ConfigureLogger();
        var args = Args.Parse();

        logger.LogInformation("Loading CSDL: {inputFile} {size}", args.InputFile, new FileInfo(args.InputFile).Length.BytesToString());
        using var reader = XmlReader.Create(args.InputFile);
        if (!CsdlReader.TryParse(reader, out var model, out var parseErrors))
        {
            foreach (var error in parseErrors)
            {
                logger.LogError("Error loading CSDL: {error}", error);
            }
            return;
        }
        logger.LogInformation("Finished loading CSDL: {inputFile}", args.InputFile);

        // creating a graph representatoin of the structural types and their relationship
        var schemaGraph = TypeGraph.FromModel(model, logger);
        logger.LogInformation("Finished creating schema graph");
        logger.LogInformation("Number of Nodes {count}", schemaGraph.NodeCount);
        logger.LogInformation("Number of Edges {count}", schemaGraph.EdgeCount);

        // check if graph is complete
        if (schemaGraph.Validate(out var errors))
        {
            foreach (var error in parseErrors)
            {
                logger.LogWarning("Error: {error}", error);
            }
        }

        // save the graph
        EnsureDirectoryExists("output");
        var graphFilePath = $"output/{Path.GetFileNameWithoutExtension(args.InputFile)}_schema_graph.txt";
        logger.LogInformation("Writing graph to {path}", graphFilePath);
        schemaGraph.WriteTo(graphFilePath);
        logger.LogInformation("Finished writing schema graph {size}", new FileInfo(graphFilePath).Length.BytesToString());

        // flatten the graph into a list of paths
        var pathsFilePath = $"output/{Path.GetFileNameWithoutExtension(args.InputFile)}_paths.txt";
        logger.LogInformation("Writing paths to {path}", pathsFilePath);
        using (var file = File.CreateText(pathsFilePath))
        {
            // foreach (var path in schemaGraph.Unfold(logger).Each(1000))
            foreach (var path in schemaGraph.Unfold(logger))
            {
                file.WriteLine("/{0}", string.Join("/", path));
            }
        }
        logger.LogInformation("Finished writing paths to {path} {size}", pathsFilePath, new FileInfo(pathsFilePath).Length.BytesToString());

        Log.CloseAndFlush();
    }


    private static ILogger<Program> ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            // .WriteTo.File("analyzer.log")
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();

        var factory = new Microsoft.Extensions.Logging.LoggerFactory().AddSerilog(Log.Logger);
        var logger = factory.CreateLogger<Program>();
        return logger;
    }

    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
