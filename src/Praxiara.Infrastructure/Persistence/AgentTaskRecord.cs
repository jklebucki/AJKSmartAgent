namespace Praxiara.Infrastructure.Persistence;

public sealed class AgentTaskRecord
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public required string Goal { get; set; }

    public required string State { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public uint RowVersion { get; set; }
}