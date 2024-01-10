class Args
{
    const string DEFAULT_INPUT = "../data/example89.csdl.xml";

    public string InputFile { get; private set; } = null!;

    public int MaxNumOfKeys { get; private set; } = 3;
    public static Args Parse()
    {
        var result = new Args { };
        var defaultArgProvided = false;
        var args = Environment.GetCommandLineArgs();
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--max-keys":
                case "-m":
                    i += 1;
                    result.MaxNumOfKeys = int.Parse(args[i]);
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
