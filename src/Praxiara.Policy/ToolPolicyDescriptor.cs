using Praxiara.Domain.Policy;

namespace Praxiara.Policy;

public sealed record ToolPolicyDescriptor(
    string ToolName,
    string DisplayName,
    RiskLevel RiskLevel,
    string Consequence,
    string? RequiredPermission = null);