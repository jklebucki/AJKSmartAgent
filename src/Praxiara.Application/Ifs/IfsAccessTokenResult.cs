namespace Praxiara.Application.Ifs;

public sealed record IfsAccessTokenResult(
    IfsAccessTokenStatus Status,
    string? AccessToken,
    string? ErrorCode);