using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Praxiara.Application.Abstractions;
using Praxiara.Application.Ifs;

namespace Praxiara.Infrastructure.Persistence;

public sealed class IfsEnvironmentConfigurationStore(
    PraxiaraDbContext database,
    TimeProvider timeProvider) : IIfsEnvironmentConfigurationStore
{
    private readonly PraxiaraDbContext _database = database;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async ValueTask<IReadOnlyList<IfsEnvironmentConfiguration>> ListAsync(CancellationToken cancellationToken)
    {
        var records = await _database.IfsEnvironments
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        return records.Select(ToConfiguration).ToArray();
    }

    public async ValueTask<IfsEnvironmentConfiguration?> FindAsync(string id, CancellationToken cancellationToken)
    {
        var record = await _database.IfsEnvironments
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return record is null ? null : ToConfiguration(record);
    }

    public async ValueTask CreateAsync(
        IfsEnvironmentConfiguration configuration,
        string actorId,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var record = ToRecord(configuration, now, now);
        _database.IfsEnvironments.Add(record);
        _database.IfsEnvironmentAuditEvents.Add(CreateAuditRecord(record, actorId, "created", now));
        await _database.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<bool> UpdateAsync(
        IfsEnvironmentConfiguration configuration,
        string actorId,
        CancellationToken cancellationToken)
    {
        var record = await _database.IfsEnvironments
            .SingleOrDefaultAsync(item => item.Id == configuration.Id, cancellationToken);
        if (record is null)
        {
            return false;
        }

        var now = _timeProvider.GetUtcNow();
        ApplyConfiguration(record, configuration, now);
        _database.IfsEnvironmentAuditEvents.Add(CreateAuditRecord(record, actorId, "updated", now));
        await _database.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async ValueTask<bool> DeleteAsync(string id, string actorId, CancellationToken cancellationToken)
    {
        var record = await _database.IfsEnvironments
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (record is null)
        {
            return false;
        }

        var now = _timeProvider.GetUtcNow();
        _database.IfsEnvironmentAuditEvents.Add(CreateAuditRecord(record, actorId, "deleted", now));
        _database.IfsEnvironments.Remove(record);
        await _database.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static IfsEnvironmentConfiguration ToConfiguration(IfsEnvironmentRecord record)
    {
        var projectionNames = JsonSerializer.Deserialize<string[]>(record.AllowedProjectionNames) ?? [];
        return new IfsEnvironmentConfiguration(
            record.Id,
            new Uri(record.BaseUri, UriKind.Absolute),
            record.Tenant,
            record.Locale,
            record.EnvironmentKind,
            new HashSet<string>(projectionNames, StringComparer.Ordinal),
            Enum.Parse<IfsAuthenticationMode>(record.AuthenticationMode, true),
            record.SecretFilePath,
            string.IsNullOrWhiteSpace(record.TokenEndpoint) ? null : new Uri(record.TokenEndpoint, UriKind.Absolute),
            record.ClientId);
    }

    private static IfsEnvironmentRecord ToRecord(
        IfsEnvironmentConfiguration configuration,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        var record = new IfsEnvironmentRecord
        {
            Id = configuration.Id,
            CreatedAt = createdAt,
        };
        ApplyConfiguration(record, configuration, updatedAt);
        return record;
    }

    private static void ApplyConfiguration(
        IfsEnvironmentRecord record,
        IfsEnvironmentConfiguration configuration,
        DateTimeOffset updatedAt)
    {
        record.BaseUri = configuration.BaseUri.AbsoluteUri;
        record.Tenant = configuration.Tenant;
        record.Locale = configuration.Locale;
        record.EnvironmentKind = configuration.EnvironmentKind;
        record.AllowedProjectionNames = JsonSerializer.Serialize(configuration.AllowedProjectionNames.Order(StringComparer.Ordinal));
        record.AuthenticationMode = configuration.AuthenticationMode.ToString();
        record.SecretFilePath = configuration.SecretFilePath;
        record.TokenEndpoint = configuration.TokenEndpoint?.AbsoluteUri;
        record.ClientId = configuration.ClientId;
        record.UpdatedAt = updatedAt;
    }

    private static IfsEnvironmentAuditRecord CreateAuditRecord(
        IfsEnvironmentRecord record,
        string actorId,
        string operation,
        DateTimeOffset occurredAt) =>
        new()
        {
            Id = Guid.CreateVersion7(occurredAt),
            EnvironmentId = record.Id,
            ActorId = actorId,
            Operation = operation,
            ConfigurationHash = CreateConfigurationHash(record),
            OccurredAt = occurredAt,
        };

    private static string CreateConfigurationHash(IfsEnvironmentRecord record)
    {
        var input = string.Join(
            '\n',
            record.Id,
            record.BaseUri,
            record.Tenant,
            record.Locale,
            record.EnvironmentKind,
            record.AllowedProjectionNames,
            record.AuthenticationMode,
            record.TokenEndpoint ?? string.Empty,
            record.ClientId ?? string.Empty);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}