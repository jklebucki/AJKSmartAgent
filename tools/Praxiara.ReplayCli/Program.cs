if (args.Length == 0 || args.Contains("--help", StringComparer.Ordinal))
{
    Console.WriteLine("Usage: praxiara-replay <verify|inspect|export> [options]");
    return 0;
}

Console.Error.WriteLine($"Command '{args[0]}' is reserved for the audit replay implementation phase.");
return 2;