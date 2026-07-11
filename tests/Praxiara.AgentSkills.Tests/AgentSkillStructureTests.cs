using System.Text.RegularExpressions;

using YamlDotNet.RepresentationModel;

namespace Praxiara.AgentSkills.Tests;

public sealed class AgentSkillStructureTests
{
    private static readonly string[] RequiredFrontmatterKeys = ["description", "name"];
    private static readonly string[] SkillScopes = ["shared-skills", "skills"];

    private static readonly Regex LocalMarkdownLinkPattern = new(
        @"\[[^\]]+\]\((?!https?://|mailto:)(?<target>[^)#]+)(?:#[^)]+)?\)",
        RegexOptions.CultureInvariant);

    [Fact]
    public void SkillPackageHasValidMetadataAndResources()
    {
        var skillDirectories = GetSkillDirectories();

        Assert.Equal(12, skillDirectories.Length);
        Assert.Equal(
            skillDirectories.Length,
            skillDirectories.Select(directory => directory.Name).Distinct(StringComparer.Ordinal).Count());

        foreach (var directory in skillDirectories)
        {
            ValidateSkill(directory);
        }
    }

    [Fact]
    public void TriggerCatalogCoversEverySkillInBothDirections()
    {
        var catalog = LoadYamlMapping(Path.Combine(GetRepositoryRoot().FullName, "tests", "agent-skills", "trigger-cases.yaml"));
        var skills = GetMapping(catalog, "skills");
        var expectedNames = GetSkillDirectories()
            .Select(directory => directory.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actualNames = skills.Children.Keys
            .Cast<YamlScalarNode>()
            .Select(node => node.Value!)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedNames, actualNames);

        foreach (var name in expectedNames)
        {
            var scenarios = GetMapping(skills, name);
            Assert.True(GetSequence(scenarios, "positive").Children.Count >= 3, $"Skill '{name}' needs at least three positive trigger cases.");
            Assert.True(GetSequence(scenarios, "negative").Children.Count >= 3, $"Skill '{name}' needs at least three negative trigger cases.");
        }
    }

    private static void ValidateSkill(DirectoryInfo directory)
    {
        var skillPath = Path.Combine(directory.FullName, "SKILL.md");
        var source = File.ReadAllText(skillPath);
        var metadata = LoadFrontmatter(source, skillPath);

        Assert.Equal(RequiredFrontmatterKeys, GetKeys(metadata));
        Assert.Equal(directory.Name, GetScalar(metadata, "name"));

        var description = GetScalar(metadata, "description");
        Assert.InRange(description.Length, 50, 320);
        Assert.Contains("Nie używaj", description, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TODO", source, StringComparison.OrdinalIgnoreCase);
        Assert.True(source.Split('\n').Length < 500, $"Skill '{directory.Name}' exceeds 500 lines.");

        var referenceFiles = Directory
            .EnumerateFiles(Path.Combine(directory.FullName, "references"), "*.md", SearchOption.TopDirectoryOnly)
            .ToArray();
        Assert.Single(referenceFiles);
        Assert.True(new FileInfo(referenceFiles[0]).Length > 0, $"Skill '{directory.Name}' has an empty reference file.");

        var openAiPath = Path.Combine(directory.FullName, "agents", "openai.yaml");
        var openAi = LoadYamlMapping(openAiPath);
        var interfaceMetadata = GetMapping(openAi, "interface");
        Assert.InRange(GetScalar(interfaceMetadata, "short_description").Length, 25, 64);
        Assert.Contains($"${directory.Name}", GetScalar(interfaceMetadata, "default_prompt"), StringComparison.Ordinal);

        foreach (Match match in LocalMarkdownLinkPattern.Matches(source))
        {
            var target = Uri.UnescapeDataString(match.Groups["target"].Value.Trim('<', '>'));
            Assert.True(File.Exists(Path.GetFullPath(target, directory.FullName)), $"Skill '{directory.Name}' has a missing link target '{target}'.");
        }

        var forbiddenFiles = Directory
            .EnumerateFiles(directory.FullName, "*.md", SearchOption.AllDirectories)
            .Where(path => Path.GetFileName(path) is "README.md" or "CHANGELOG.md")
            .ToArray();
        Assert.Empty(forbiddenFiles);
    }

    private static YamlMappingNode LoadFrontmatter(string source, string path)
    {
        const string delimiter = "---";
        using var reader = new StringReader(source);

        if (!string.Equals(reader.ReadLine(), delimiter, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Skill '{path}' does not start with YAML frontmatter.");
        }

        var lines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null && !string.Equals(line, delimiter, StringComparison.Ordinal))
        {
            lines.Add(line);
        }

        if (line is null)
        {
            throw new InvalidDataException($"Skill '{path}' has unterminated YAML frontmatter.");
        }

        return LoadYamlMapping(string.Join(Environment.NewLine, lines));
    }

    private static YamlMappingNode LoadYamlMapping(string pathOrYaml)
    {
        var yaml = File.Exists(pathOrYaml) ? File.ReadAllText(pathOrYaml) : pathOrYaml;
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        return Assert.IsType<YamlMappingNode>(Assert.Single(stream.Documents).RootNode);
    }

    private static string[] GetKeys(YamlMappingNode mapping) =>
        mapping.Children.Keys
            .Cast<YamlScalarNode>()
            .Select(node => node.Value!)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string GetScalar(YamlMappingNode mapping, string key) =>
        Assert.IsType<YamlScalarNode>(mapping.Children[new YamlScalarNode(key)]).Value!;

    private static YamlMappingNode GetMapping(YamlMappingNode mapping, string key) =>
        Assert.IsType<YamlMappingNode>(mapping.Children[new YamlScalarNode(key)]);

    private static YamlSequenceNode GetSequence(YamlMappingNode mapping, string key) =>
        Assert.IsType<YamlSequenceNode>(mapping.Children[new YamlScalarNode(key)]);

    private static DirectoryInfo[] GetSkillDirectories()
    {
        var agentsRoot = Path.Combine(GetRepositoryRoot().FullName, ".agents");
        return SkillScopes
            .SelectMany(scope => new DirectoryInfo(Path.Combine(agentsRoot, scope)).EnumerateDirectories())
            .OrderBy(directory => directory.Name, StringComparer.Ordinal)
            .ToArray();
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