using Praxiara.Contracts.Browser;

namespace Praxiara.Contracts.Approvals;

public sealed record ApprovalRequest(
    Guid ApprovalId,
    Guid TaskId,
    string UserId,
    ProposedToolCall ToolCall,
    ApprovalPreview Preview,
    string ActionHash,
    DateTimeOffset ExpiresAt);