namespace Praxiara.Application.Ifs;

public sealed record IfsEnvironmentConfiguration(
    string Id,
    Uri BaseUri,
    string Tenant,
    string Locale,
    string EnvironmentKind,
    IReadOnlySet<string> AllowedProjectionNames,
    IfsAuthenticationMode AuthenticationMode,
    string SecretFilePath,
    Uri? TokenEndpoint,
    string? ClientId);