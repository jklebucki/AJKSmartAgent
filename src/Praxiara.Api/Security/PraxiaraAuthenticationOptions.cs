namespace Praxiara.Api.Security;

public sealed class PraxiaraAuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string? Authority { get; init; }

    public string? Audience { get; init; }

    public string? ClientId { get; init; }

    public bool RequireHttpsMetadata { get; init; } = true;

    public int ClockSkewSeconds { get; init; } = 30;

    public bool IsConfigured =>
        Uri.TryCreate(Authority, UriKind.Absolute, out var authority) &&
        (authority.Scheme == Uri.UriSchemeHttp || authority.Scheme == Uri.UriSchemeHttps) &&
        !string.IsNullOrWhiteSpace(Audience) &&
        !string.IsNullOrWhiteSpace(ClientId) &&
        ClockSkewSeconds is >= 0 and <= 300;
}