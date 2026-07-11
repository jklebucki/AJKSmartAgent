using System.Text.RegularExpressions;

namespace Praxiara.ArchitectureTests;

public sealed class SourceLayoutTests
{
    private static readonly Regex NamedTypePattern = new(
        "^[ \\t]*(?:(?:public|internal|file|protected|private|sealed|abstract|static|partial|readonly|ref|unsafe|new)\\s+)*(?:class|interface|enum|struct|record(?:\\s+(?:class|struct))?)\\s+(?<name>[A-Za-z_]\\w*)",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    private static readonly Regex DelegatePattern = new(
        "^[ \\t]*(?:(?:public|internal|file|protected|private|sealed|abstract|static|partial|readonly|ref|unsafe|new)\\s+)*delegate\\s+.+?\\s+(?<name>[A-Za-z_]\\w*)\\s*\\(",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    private static readonly Regex NamespacePattern = new(
        "^namespace\\s+[A-Za-z_]\\w*(?:\\.[A-Za-z_]\\w*)*;\\s*$",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    [Fact]
    public void HandwrittenSourceFileContainsOneCorrectlyNamedType()
    {
        var violations = EnumerateSourceFiles()
            .Select(InspectTypeLayout)
            .Where(result => result is not null)
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void NamespacedSourceFileUsesFileScopedNamespace()
    {
        var violations = EnumerateSourceFiles()
            .Where(file => File.ReadAllText(file).Contains("namespace ", StringComparison.Ordinal))
            .Where(file => !NamespacePattern.IsMatch(File.ReadAllText(file)))
            .Select(file => Path.GetRelativePath(GetRepositoryRoot().FullName, file))
            .ToArray();

        Assert.Empty(violations);
    }

    private static string? InspectTypeLayout(string file)
    {
        var source = File.ReadAllText(file);
        var declaredNames = NamedTypePattern.Matches(source)
            .Concat(DelegatePattern.Matches(source))
            .Select(match => match.Groups["name"].Value)
            .ToArray();
        var relativePath = Path.GetRelativePath(GetRepositoryRoot().FullName, file);

        if (declaredNames.Length > 1)
        {
            return $"{relativePath} declares {declaredNames.Length} named types.";
        }

        if (declaredNames.Length == 0)
        {
            return string.Equals(Path.GetFileName(file), "Program.cs", StringComparison.Ordinal)
                ? null
                : $"{relativePath} does not declare a named type.";
        }

        var declaredName = declaredNames[0];
        var fileName = Path.GetFileNameWithoutExtension(file);

        return string.Equals(fileName, declaredName, StringComparison.Ordinal)
            ? null
            : $"{relativePath} declares '{declaredName}' but its file name is '{fileName}.cs'.";
    }

    private static IEnumerable<string> EnumerateSourceFiles()
    {
        var root = GetRepositoryRoot().FullName;
        var sourceRoots = new[] { "src", "tests", "tools" };

        return sourceRoots
            .SelectMany(directory => Directory.EnumerateFiles(
                Path.Combine(root, directory),
                "*.cs",
                SearchOption.AllDirectories))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(file => !file.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal);
    }

    private static DirectoryInfo GetRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Praxiara.slnx")))
            {
                return directory;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the Praxiara repository root.");
    }
}