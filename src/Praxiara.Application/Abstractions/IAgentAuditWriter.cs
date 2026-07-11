namespace Praxiara.Application.Abstractions;

public interface IAgentAuditWriter
{
    ValueTask AppendAsync(AgentAuditEntry entry, CancellationToken cancellationToken);
}