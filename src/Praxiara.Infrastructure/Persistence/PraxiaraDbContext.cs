using Microsoft.EntityFrameworkCore;

namespace Praxiara.Infrastructure.Persistence;

public sealed class PraxiaraDbContext(DbContextOptions<PraxiaraDbContext> options) : DbContext(options)
{
    public DbSet<AgentTaskRecord> AgentTasks => Set<AgentTaskRecord>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgentTaskRecord>(entity =>
        {
            entity.ToTable("agent_tasks");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.UserId).HasMaxLength(256).IsRequired();
            entity.Property(item => item.Goal).HasMaxLength(8000).IsRequired();
            entity.Property(item => item.State).HasMaxLength(64).IsRequired();
            entity.Property(item => item.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Type).HasMaxLength(512).IsRequired();
            entity.Property(item => item.Payload).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(item => new { item.ProcessedAt, item.OccurredAt });
        });
    }
}

public sealed class AgentTaskRecord
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public required string Goal { get; set; }

    public required string State { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public uint RowVersion { get; set; }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public required string Type { get; set; }

    public required string Payload { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}