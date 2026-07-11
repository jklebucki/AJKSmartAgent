namespace Praxiara.Integrations.IFS;

public sealed record IfsEnvironmentProfile(
    string Id,
    Uri BaseUri,
    string Tenant,
    string Locale,
    string EnvironmentKind,
    IReadOnlySet<string> AllowedProjectionNames);