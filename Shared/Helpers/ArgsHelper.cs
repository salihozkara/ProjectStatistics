namespace Shared.Helpers;

public static class ArgsHelper
{
    public static Args Parse(string[] args)
    {
        var result = new Args();

        var argsDictionary = args
            .Select(x => x.Split('='))
            .ToDictionary(x => x[0], x => x[1]);

        result.ComputerCount =
            int.Parse(argsDictionary.GetVal("-c") ?? argsDictionary.GetVal("--computer-count") ?? "1");
        result.ComputerIndex =
            int.Parse(argsDictionary.GetVal("-i") ?? argsDictionary.GetVal("--computer-index") ?? "0");

        return result;
    }

    private static string? GetVal(this IReadOnlyDictionary<string, string> args, string key)
    {
        return args.ContainsKey(key) ? args[key] : null;
    }
}