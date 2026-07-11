namespace Praxiara.Skills;

public sealed record SkillInput
{
    public required string Type { get; init; }

    public bool Required { get; init; }

    public bool Sensitive { get; init; }
}