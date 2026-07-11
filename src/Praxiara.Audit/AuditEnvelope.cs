using System.Text.Json;

namespace Praxiara.Audit;

public sealed record AuditEnvelope(
    Guid EventId,
    Guid TaskId,
    Guid SessionId,
    DateTimeOffset Timestamp,
    string EventType,
    JsonElement Payload,
    string? PreviousHash,
    string Hash);