using Praxiara.Contracts.Browser;
using Praxiara.Domain.Tasks;

namespace Praxiara.ArchitectureTests;

public sealed class DependencyRulesTests
{
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

    public static TheoryData<Type> FoundationAssemblies => new()
    {
        typeof(AgentTask),
        typeof(BrowserObservation)
    };
}