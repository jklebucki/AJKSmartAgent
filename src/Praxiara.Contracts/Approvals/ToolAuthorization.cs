namespace Praxiara.Contracts.Approvals;

public sealed record ToolAuthorization(
    bool Allowed,
    bool RequiresApproval,
    string? DenialReason,
    ApprovalPreview? Preview)
{
    public static ToolAuthorization Denied(string reason) => new(false, false, reason, null);

    public static ToolAuthorization Automatic() => new(true, false, null, null);

    public static ToolAuthorization WithApproval(ApprovalPreview preview) => new(true, true, null, preview);
}