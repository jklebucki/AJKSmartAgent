using Praxiara.Contracts.Approvals;
using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public sealed record AgentAuditEntry(
    Guid TaskId,
    Guid SessionId,
    DateTimeOffset Timestamp,
    BrowserObservation ObservationBefore,
    ProposedToolCall ToolCall,
    ToolAuthorization Authorization,
    ToolExecutionResult? ExecutionResult,
    VerificationResult? VerificationResult);