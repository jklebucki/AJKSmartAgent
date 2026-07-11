namespace Praxiara.Application.Abstractions;

public sealed record ToolPolicyContext(
    Guid TaskId,
    string UserId,
    string Environment,
    Uri CurrentUrl,
    IReadOnlySet<string> Permissions);