using System.Security.Cryptography;
using System.Text;
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

public static class AuditHashChain
{
    public static AuditEnvelope Create(
        Guid taskId,
        Guid sessionId,
        string eventType,
        JsonElement payload,
        string? previousHash,
        DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        var eventId = Guid.CreateVersion7(timestamp);
        var canonicalInput = string.Join(
            '\n',
            eventId,
            taskId,
            sessionId,
            timestamp.ToUniversalTime().ToString("O"),
            eventType,
            payload.GetRawText(),
            previousHash ?? string.Empty);
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalInput)));

        return new AuditEnvelope(eventId, taskId, sessionId, timestamp, eventType, payload.Clone(), previousHash, hash);
    }
}

public interface IAuditEnvelopeSink
{
    ValueTask AppendAsync(AuditEnvelope envelope, CancellationToken cancellationToken);
}