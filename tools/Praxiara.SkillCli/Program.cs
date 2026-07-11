if (args.Length == 0 || args.Contains("--help", StringComparer.Ordinal))
{
    Console.WriteLine("Usage: praxiara-skill <validate|record|publish> [options]");
    return 0;
}

Console.Error.WriteLine($"Command '{args[0]}' is reserved for the skill tooling implementation phase.");
return 2;