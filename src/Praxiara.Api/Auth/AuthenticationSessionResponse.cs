using System.Text.Json.Serialization;

namespace Praxiara.Api.Auth;

public sealed record AuthenticationSessionResponse(
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles);