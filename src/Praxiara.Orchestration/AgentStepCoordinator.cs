using Praxiara.Application.Abstractions;
using Praxiara.Contracts.Approvals;
using Praxiara.Contracts.Browser;

namespace Praxiara.Orchestration;

public sealed class AgentStepCoordinator(
    IObservationProvider observationProvider,
    IAgentPlanner planner,
    IToolPolicy policy,
    IToolExecutor executor,
    IActionVerifier verifier,
    IAgentAuditWriter auditWriter,
    TimeProvider timeProvider)
{
    public async ValueTask<AgentStepOutcome> ExecuteNextStepAsync(
        AgentStepRequest request,
        CancellationToken cancellationToken)
    {
        var observationBefore = await observationProvider.ObserveAsync(request.SessionId, cancellationToken);
        var planningContext = new AgentPlanningContext(
            request.TaskId,
            request.UserGoal,
            observationBefore,
            request.AllowedTools,
            request.ApplicableSkillIds);

        var decision = await planner.DecideAsync(planningContext, cancellationToken);
        if (decision.IsComplete)
        {
            return AgentStepOutcome.Completed(decision.Summary);
        }

        if (decision.ToolCall is not { } toolCall)
        {
            return AgentStepOutcome.Failed("planner_missing_action", "The planner returned neither completion nor a tool call.");
        }

        if (toolCall.ExpectedPageRevision != observationBefore.Revision)
        {
            return AgentStepOutcome.Reobserve("The proposed action was based on a stale page revision.");
        }

        var policyContext = new ToolPolicyContext(
            request.TaskId,
            request.UserId,
            request.Environment,
            observationBefore.Url,
            request.Permissions);
        var authorization = await policy.AuthorizeAsync(policyContext, toolCall, cancellationToken);

        if (!authorization.Allowed)
        {
            await WriteAuditAsync(request, observationBefore, toolCall, authorization, null, null, cancellationToken);
            return AgentStepOutcome.Denied(authorization.DenialReason ?? "The action was denied by policy.");
        }

        if (authorization.RequiresApproval)
        {
            await WriteAuditAsync(request, observationBefore, toolCall, authorization, null, null, cancellationToken);
            return AgentStepOutcome.AwaitingApproval(authorization.Preview!);
        }

        var execution = await executor.ExecuteAsync(request.SessionId, toolCall, cancellationToken);
        var observationAfter = await observationProvider.ObserveAsync(request.SessionId, cancellationToken);
        var verification = await verifier.VerifyAsync(toolCall, execution, observationAfter, cancellationToken);

        await WriteAuditAsync(
            request,
            observationBefore,
            toolCall,
            authorization,
            execution,
            verification,
            cancellationToken);

        return verification.Succeeded
            ? AgentStepOutcome.Executed(verification.Detail)
            : AgentStepOutcome.Failed(verification.Code, verification.Detail ?? "Postcondition verification failed.");
    }

    private ValueTask WriteAuditAsync(
        AgentStepRequest request,
        BrowserObservation observation,
        ProposedToolCall toolCall,
        ToolAuthorization authorization,
        ToolExecutionResult? executionResult,
        VerificationResult? verificationResult,
        CancellationToken cancellationToken) =>
        auditWriter.AppendAsync(
            new AgentAuditEntry(
                request.TaskId,
                request.SessionId,
                timeProvider.GetUtcNow(),
                observation,
                toolCall,
                authorization,
                executionResult,
                verificationResult),
            cancellationToken);
}

public sealed record AgentStepRequest(
    Guid TaskId,
    Guid SessionId,
    string UserId,
    string UserGoal,
    string Environment,
    IReadOnlySet<string> AllowedTools,
    IReadOnlySet<string> Permissions,
    IReadOnlyList<string> ApplicableSkillIds);

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

public enum AgentStepStatus
{
    Executed,
    AwaitingApproval,
    Reobserve,
    Denied,
    Completed,
    Failed
}