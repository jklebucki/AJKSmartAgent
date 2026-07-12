namespace Praxiara.Integrations.IFS;

internal sealed record IfsCachedAccessToken(string Value, DateTimeOffset ExpiresAt);