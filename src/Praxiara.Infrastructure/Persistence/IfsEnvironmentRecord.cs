namespace Praxiara.Infrastructure.Persistence;

public sealed class IfsEnvironmentRecord
{
    public string Id { get; set; } = string.Empty;

    public string BaseUri { get; set; } = string.Empty;

    public string Tenant { get; set; } = string.Empty;

    public string Locale { get; set; } = string.Empty;

    public string EnvironmentKind { get; set; } = string.Empty;

    public string AllowedProjectionNames { get; set; } = string.Empty;

    public string AuthenticationMode { get; set; } = string.Empty;

    public string SecretFilePath { get; set; } = string.Empty;

    public string? TokenEndpoint { get; set; }

    public string? ClientId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public uint RowVersion { get; set; }
}