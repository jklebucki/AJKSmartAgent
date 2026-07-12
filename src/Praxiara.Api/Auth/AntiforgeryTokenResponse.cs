using System.Text.Json.Serialization;

namespace Praxiara.Api.Auth;

public sealed record AntiforgeryTokenResponse(
    [property: JsonPropertyName("requestToken")] string RequestToken);