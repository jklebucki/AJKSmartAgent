using System.Xml.Linq;

using Praxiara.Contracts.Browser;
using Praxiara.Domain.Tasks;

namespace Praxiara.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> AllowedProjectReferences =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            ["Praxiara.Domain"] = [],
            ["Praxiara.Contracts"] = [],
            ["Praxiara.Application"] = ["Praxiara.Domain", "Praxiara.Contracts"],
            ["Praxiara.Orchestration"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Policy"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Browser"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Skills"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Llm"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Integrations.IFS"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Audit"] = ["Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Browser.Playwright"] = ["Praxiara.Browser", "Praxiara.Application", "Praxiara.Contracts", "Praxiara.Domain"],
            ["Praxiara.Infrastructure"] =
            [
                "Praxiara.Application",
                "Praxiara.Contracts",
                "Praxiara.Domain",
                "Praxiara.Orchestration",
                "Praxiara.Policy",
                "Praxiara.Browser",
                "Praxiara.Skills",
                "Praxiara.Llm",
                "Praxiara.Integrations.IFS",
                "Praxiara.Audit"
            ],
            ["Praxiara.ServiceDefaults"] = [],
            ["Praxiara.Api"] =
            [
                "Praxiara.ServiceDefaults",
                "Praxiara.Application",
                "Praxiara.Contracts",
                "Praxiara.Domain",
                "Praxiara.Orchestration",
                "Praxiara.Policy",
                "Praxiara.Browser",
                "Praxiara.Skills",
                "Praxiara.Llm",
                "Praxiara.Integrations.IFS",
                "Praxiara.Audit",
                "Praxiara.Infrastructure"
            ],
            ["Praxiara.Browser.Worker"] =
            [
                "Praxiara.ServiceDefaults",
                "Praxiara.Browser",
                "Praxiara.Browser.Playwright",
                "Praxiara.Contracts",
                "Praxiara.Audit"
            ],
            ["Praxiara.Orchestrator.Worker"] =
            [
                "Praxiara.ServiceDefaults",
                "Praxiara.Application",
                "Praxiara.Contracts",
                "Praxiara.Domain",
                "Praxiara.Orchestration",
                "Praxiara.Policy",
                "Praxiara.Browser",
                "Praxiara.Skills",
                "Praxiara.Llm",
                "Praxiara.Integrations.IFS",
                "Praxiara.Audit",
                "Praxiara.Infrastructure"
            ],
            ["Praxiara.AppHost"] =
            [
                "Praxiara.Api",
                "Praxiara.Browser.Worker",
                "Praxiara.Orchestrator.Worker"
            ]
        };

    [Theory]
    [MemberData(nameof(FoundationAssemblies))]
    public void FoundationAssemblyDoesNotReferenceHigherLevelPraxiaraAssembly(Type markerType)
    {
        var references = markerType.Assembly
            .GetReferencedAssemblies()
            .Where(reference => reference.Name?.StartsWith("Praxiara.", StringComparison.Ordinal) == true)
            .Select(reference => reference.Name)
            .ToArray();

        Assert.Empty(references);
    }

    [Fact]
    public void SourceProjectReferencesRespectModuleBoundaries()
    {
        var sourceProjects = Directory
            .EnumerateFiles(Path.Combine(GetRepositoryRoot().FullName, "src"), "*.csproj", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var violations = new List<string>();

        foreach (var projectFile in sourceProjects)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFile);

            if (!AllowedProjectReferences.TryGetValue(projectName, out var allowedReferences))
            {
                violations.Add($"{projectName} is missing from the architecture allowlist.");
                continue;
            }

            var referencedProjects = XDocument.Load(projectFile)
                .Descendants("ProjectReference")
                .Select(reference => reference.Attribute("Include")?.Value)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => Path.GetFileNameWithoutExtension(
                    path!.Replace('\\', Path.DirectorySeparatorChar)));

            violations.AddRange(referencedProjects
                .Where(reference => !allowedReferences.Contains(reference))
                .Select(reference => $"{projectName} must not reference {reference}."));
        }

        var missingProjects = AllowedProjectReferences.Keys
            .Except(sourceProjects.Select(Path.GetFileNameWithoutExtension), StringComparer.Ordinal)
            .Select(project => $"{project} is present in the architecture allowlist but missing from src.");
        violations.AddRange(missingProjects);

        Assert.Empty(violations);
    }

    [Theory]
    [InlineData("Praxiara.Domain", "Praxiara.Application")]
    [InlineData("Praxiara.Contracts", "Praxiara.Domain")]
    [InlineData("Praxiara.Policy", "Praxiara.Orchestration")]
    [InlineData("Praxiara.Browser", "Praxiara.Browser.Playwright")]
    [InlineData("Praxiara.Browser.Worker", "Praxiara.Infrastructure")]
    [InlineData("Praxiara.Orchestrator.Worker", "Praxiara.Browser.Playwright")]
    public void SecurityAndModuleBoundaryRejectsForbiddenReference(string project, string reference)
    {
        Assert.DoesNotContain(reference, AllowedProjectReferences[project]);
    }

    public static TheoryData<Type> FoundationAssemblies => new()
    {
        typeof(AgentTask),
        typeof(BrowserObservation)
    };

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