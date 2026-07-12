namespace Praxiara.Contracts.Ifs;

public sealed record IfsEnvironmentCreateRequest(
    string Id,
    string BaseUri,
    string Tenant,
    string Locale,
    string EnvironmentKind,
    IReadOnlyList<string> AllowedProjectionNames,
    string AuthenticationMode,
    string SecretFilePath,
    string? TokenEndpoint,
    string? ClientId);