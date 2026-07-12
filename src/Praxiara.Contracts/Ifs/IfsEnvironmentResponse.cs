using System.Text.Json.Serialization;

namespace Praxiara.Contracts.Ifs;

public sealed record IfsEnvironmentResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("baseUri")] string BaseUri,
    [property: JsonPropertyName("tenant")] string Tenant,
    [property: JsonPropertyName("locale")] string Locale,
    [property: JsonPropertyName("environmentKind")] string EnvironmentKind,
    [property: JsonPropertyName("allowedProjectionNames")] IReadOnlyList<string> AllowedProjectionNames,
    [property: JsonPropertyName("authenticationMode")] string AuthenticationMode,
    [property: JsonPropertyName("tokenEndpoint")] string? TokenEndpoint,
    [property: JsonPropertyName("clientId")] string? ClientId,
    [property: JsonPropertyName("isSecretReferenceConfigured")] bool IsSecretReferenceConfigured);