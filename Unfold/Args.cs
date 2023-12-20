class Args
{
    const string DEFAULT_INPUT = "../data/example89.csdl.xml";
    // const string DEFAULT_INPUT = "../data/directory.csdl.xml";
    // const string DEFAULT_INPUT = "../data/graph.csdl.xml";

    public string InputFile { get; private set; } = null!;
    public int MaxKeys { get; private set; }

    // public bool ShowPaths { get; private set; }
    // public bool HideTree { get; private set; }

    public static Args Parse()
    {
        var result = new Args { MaxKeys = 3 };
        var defaultArgProvided = false;
        var args = Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                // case "--paths":
                // case "-p":
                //     result.ShowPaths = true;
                //     break;
                // case "--no-tree":
                // case "-t":
                //         result.HideTree = true;
                // break;
                case "--max-keys":
                case "-m":
                    i += 1;
                    result.MaxKeys = int.Parse(args[i]);
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
        result.InputFile ??= DEFAULT_INPUT;
        return result;
    }
}
