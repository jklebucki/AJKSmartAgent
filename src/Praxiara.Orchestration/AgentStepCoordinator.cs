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