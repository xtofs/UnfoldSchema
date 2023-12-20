class Args
{
    // const string DEFAULT_INPUT = "../data/example89.csdl.xml";
    // const string DEFAULT_INPUT = "../data/directory.csdl.xml";
    const string DEFAULT_INPUT = "../data/graph.csdl.xml";

    public string InputFile { get; private set; } = null!;
    public bool ShowPaths { get; private set; }
    public bool HideTree { get; private set; }

    public static Args Parse()
    {
        var result = new Args();
        var defaultArgProvided = false;
        var args = Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--paths":
                case "-p":
                    result.ShowPaths = true;
                    break;
                case "--no-tree":
                case "-t":
                    result.HideTree = true;
                    break;
                case string s when s.StartsWith("--"):
                    throw new Exception($"unknown option {arg}");
                default:
                    if (defaultArgProvided)
                    {
                        throw new Exception($"two default arguments provided");
                    }
                    else
                    {
                        result.InputFile = arg;
                        defaultArgProvided = true;
                    }
                    break;
            }
        }
        if (result.InputFile == null)
        {
            result.InputFile = DEFAULT_INPUT;
        }
        return result;
    }
}
