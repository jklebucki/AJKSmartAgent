namespace Praxiara.Audit;

public interface IAuditEnvelopeSink
{
    ValueTask AppendAsync(AuditEnvelope envelope, CancellationToken cancellationToken);
}