namespace Praxiara.Application.Ifs;

public sealed record IfsMetadataReadResult(
    IfsMetadataReadStatus Status,
    string? Content,
    string? ContentType,
    string? ErrorCode);