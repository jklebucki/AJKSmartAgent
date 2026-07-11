namespace Praxiara.Contracts.Approvals;

public sealed record ApprovalPreview(
    string Operation,
    string Environment,
    IReadOnlyDictionary<string, string> Facts,
    string Consequence,
    bool RequiresReauthentication);