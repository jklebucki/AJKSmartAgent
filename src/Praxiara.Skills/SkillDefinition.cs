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

public sealed record SkillInput
{
    public required string Type { get; init; }

    public bool Required { get; init; }

    public bool Sensitive { get; init; }
}

public sealed record SkillStep
{
    public required string Id { get; init; }

    public required string Action { get; init; }

    public Dictionary<string, object?> Target { get; init; } = new(StringComparer.Ordinal);

    public Dictionary<string, object?> Arguments { get; init; } = new(StringComparer.Ordinal);

    public List<string> Assertions { get; init; } = [];

    public bool RequiresApproval { get; init; }
}

public sealed record SkillRisk
{
    public RiskLevel DefaultLevel { get; init; } = RiskLevel.R0ReadOnly;

    public RiskLevel FinalActionLevel { get; init; } = RiskLevel.R0ReadOnly;
}