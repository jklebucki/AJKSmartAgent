using Microsoft.EntityFrameworkCore;

namespace Praxiara.Infrastructure.Persistence;

public sealed class PraxiaraDbContext(DbContextOptions<PraxiaraDbContext> options) : DbContext(options)
{
    public DbSet<AgentTaskRecord> AgentTasks => Set<AgentTaskRecord>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<IfsEnvironmentRecord> IfsEnvironments => Set<IfsEnvironmentRecord>();

    public DbSet<IfsEnvironmentAuditRecord> IfsEnvironmentAuditEvents => Set<IfsEnvironmentAuditRecord>();

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

        modelBuilder.Entity<IfsEnvironmentRecord>(entity =>
        {
            entity.ToTable("ifs_environments");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(64);
            entity.Property(item => item.BaseUri).HasMaxLength(2048).IsRequired();
            entity.Property(item => item.Tenant).HasMaxLength(128).IsRequired();
            entity.Property(item => item.Locale).HasMaxLength(32).IsRequired();
            entity.Property(item => item.EnvironmentKind).HasMaxLength(32).IsRequired();
            entity.Property(item => item.AllowedProjectionNames).HasColumnType("jsonb").IsRequired();
            entity.Property(item => item.AuthenticationMode).HasMaxLength(32).IsRequired();
            entity.Property(item => item.SecretFilePath).HasMaxLength(1024).IsRequired();
            entity.Property(item => item.TokenEndpoint).HasMaxLength(2048);
            entity.Property(item => item.ClientId).HasMaxLength(256);
            entity.Property(item => item.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<IfsEnvironmentAuditRecord>(entity =>
        {
            entity.ToTable("ifs_environment_audit_events");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EnvironmentId).HasMaxLength(64).IsRequired();
            entity.Property(item => item.ActorId).HasMaxLength(256).IsRequired();
            entity.Property(item => item.Operation).HasMaxLength(32).IsRequired();
            entity.Property(item => item.ConfigurationHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(item => new { item.EnvironmentId, item.OccurredAt });
        });
    }
}