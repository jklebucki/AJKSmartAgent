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