using Praxiara.Contracts.Approvals;
using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public interface IObservationProvider
{
    ValueTask<BrowserObservation> ObserveAsync(Guid sessionId, CancellationToken cancellationToken);
}

public interface IAgentPlanner
{
    ValueTask<PlannerDecision> DecideAsync(AgentPlanningContext context, CancellationToken cancellationToken);
}

public interface IToolPolicy
{
    ValueTask<ToolAuthorization> AuthorizeAsync(
        ToolPolicyContext context,
        ProposedToolCall toolCall,
        CancellationToken cancellationToken);
}

public interface IToolExecutor
{
    ValueTask<ToolExecutionResult> ExecuteAsync(
        Guid sessionId,
        ProposedToolCall toolCall,
        CancellationToken cancellationToken);
}

public interface IActionVerifier
{
    ValueTask<VerificationResult> VerifyAsync(
        ProposedToolCall toolCall,
        ToolExecutionResult executionResult,
        BrowserObservation observationAfter,
        CancellationToken cancellationToken);
}

public interface IAgentAuditWriter
{
    ValueTask AppendAsync(AgentAuditEntry entry, CancellationToken cancellationToken);
}

public sealed record AgentPlanningContext(
    Guid TaskId,
    string UserGoal,
    BrowserObservation Observation,
    IReadOnlySet<string> AllowedTools,
    IReadOnlyList<string> ApplicableSkillIds);

public sealed record PlannerDecision(ProposedToolCall? ToolCall, bool IsComplete, string Summary);

public sealed record ToolPolicyContext(
    Guid TaskId,
    string UserId,
    string Environment,
    Uri CurrentUrl,
    IReadOnlySet<string> Permissions);

public sealed record VerificationResult(bool Succeeded, string Code, string? Detail);

public sealed record AgentAuditEntry(
    Guid TaskId,
    Guid SessionId,
    DateTimeOffset Timestamp,
    BrowserObservation ObservationBefore,
    ProposedToolCall ToolCall,
    ToolAuthorization Authorization,
    ToolExecutionResult? ExecutionResult,
    VerificationResult? VerificationResult);