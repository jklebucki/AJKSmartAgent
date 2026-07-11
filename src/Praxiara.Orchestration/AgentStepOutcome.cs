using Praxiara.Contracts.Approvals;

namespace Praxiara.Orchestration;

public sealed record AgentStepOutcome(
    AgentStepStatus Status,
    string? Code,
    string? Detail,
    ApprovalPreview? ApprovalPreview)
{
    public static AgentStepOutcome Completed(string detail) => new(AgentStepStatus.Completed, null, detail, null);

    public static AgentStepOutcome Executed(string? detail) => new(AgentStepStatus.Executed, null, detail, null);

    public static AgentStepOutcome AwaitingApproval(ApprovalPreview preview) =>
        new(AgentStepStatus.AwaitingApproval, null, null, preview);

    public static AgentStepOutcome Denied(string detail) => new(AgentStepStatus.Denied, "policy_denied", detail, null);

    public static AgentStepOutcome Reobserve(string detail) => new(AgentStepStatus.Reobserve, "stale_observation", detail, null);

    public static AgentStepOutcome Failed(string code, string detail) => new(AgentStepStatus.Failed, code, detail, null);
}