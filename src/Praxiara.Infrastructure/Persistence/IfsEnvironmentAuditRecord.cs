namespace Praxiara.Infrastructure.Persistence;

public sealed class IfsEnvironmentAuditRecord
{
    public Guid Id { get; set; }

    public required string EnvironmentId { get; set; }

    public required string ActorId { get; set; }

    public required string Operation { get; set; }

    public required string ConfigurationHash { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}