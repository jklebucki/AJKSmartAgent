namespace Praxiara.Application.Ifs;

public sealed record IfsEnvironmentMutationResult(
    IfsEnvironmentMutationStatus Status,
    IfsEnvironmentConfiguration? Configuration,
    string? ErrorCode);