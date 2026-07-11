using Praxiara.Domain.Policy;

namespace Praxiara.Skills;

public sealed record SkillDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Version { get; init; }

    public required string Site { get; init; }

    public SkillExecutionMode DefaultMode { get; init; } = SkillExecutionMode.Observe;

    public Dictionary<string, SkillInput> Inputs { get; init; } = new(StringComparer.Ordinal);

    public List<string> RequiredPermissions { get; init; } = [];

    public SkillRisk Risk { get; init; } = new();

    public List<SkillStep> Steps { get; init; } = [];

    public List<string> Preconditions { get; init; } = [];

    public List<string> Postconditions { get; init; } = [];

    public Dictionary<string, string> Recovery { get; init; } = new(StringComparer.Ordinal);
}