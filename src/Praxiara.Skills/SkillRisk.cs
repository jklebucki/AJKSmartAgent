using Praxiara.Domain.Policy;

namespace Praxiara.Skills;

public sealed record SkillRisk
{
    public RiskLevel DefaultLevel { get; init; } = RiskLevel.R0ReadOnly;

    public RiskLevel FinalActionLevel { get; init; } = RiskLevel.R0ReadOnly;
}