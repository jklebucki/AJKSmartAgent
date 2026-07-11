namespace Praxiara.Skills;

public sealed record SkillStep
{
    public required string Id { get; init; }

    public required string Action { get; init; }

    public Dictionary<string, object?> Target { get; init; } = new(StringComparer.Ordinal);

    public Dictionary<string, object?> Arguments { get; init; } = new(StringComparer.Ordinal);

    public List<string> Assertions { get; init; } = [];

    public bool RequiresApproval { get; init; }
}