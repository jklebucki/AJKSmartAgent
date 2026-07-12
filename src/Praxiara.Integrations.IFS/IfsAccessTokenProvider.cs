using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

using Praxiara.Application.Abstractions;
using Praxiara.Application.Ifs;

namespace Praxiara.Integrations.IFS;

public sealed class IfsAccessTokenProvider(
    IHttpClientFactory httpClientFactory,
    TimeProvider timeProvider) : IIfsAccessTokenProvider
{
    private static readonly ConcurrentDictionary<string, IfsCachedAccessToken> Cache = new(StringComparer.Ordinal);
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async ValueTask<IfsAccessTokenResult> GetAccessTokenAsync(
        IfsEnvironmentConfiguration environment,
        CancellationToken cancellationToken)
    {
        if (environment.AuthenticationMode is IfsAuthenticationMode.BearerTokenFile)
        {
            return await ReadBearerTokenFileAsync(environment.SecretFilePath, cancellationToken);
        }

        if (Cache.TryGetValue(environment.Id, out var cached) &&
            cached.ExpiresAt > _timeProvider.GetUtcNow().AddMinutes(1))
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.Success, cached.Value, null);
        }

        return await RequestClientCredentialsTokenAsync(environment, cancellationToken);
    }

    private static async ValueTask<IfsAccessTokenResult> ReadBearerTokenFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = (await File.ReadAllTextAsync(path, cancellationToken)).Trim();
            return string.IsNullOrWhiteSpace(token)
                ? new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_bearer_token_file_empty")
                : new IfsAccessTokenResult(IfsAccessTokenStatus.Success, token, null);
        }
        catch (IOException)
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_bearer_token_file_unavailable");
        }
        catch (UnauthorizedAccessException)
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_bearer_token_file_unavailable");
        }
    }

    private async ValueTask<IfsAccessTokenResult> RequestClientCredentialsTokenAsync(
        IfsEnvironmentConfiguration environment,
        CancellationToken cancellationToken)
    {
        if (environment.TokenEndpoint is null || string.IsNullOrWhiteSpace(environment.ClientId))
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_client_credentials_incomplete");
        }

        string clientSecret;
        try
        {
            clientSecret = (await File.ReadAllTextAsync(environment.SecretFilePath, cancellationToken)).Trim();
        }
        catch (IOException)
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_client_secret_file_unavailable");
        }
        catch (UnauthorizedAccessException)
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_client_secret_file_unavailable");
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.CredentialsUnavailable, null, "ifs_client_secret_file_empty");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, environment.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "openid microprofile-jwt"),
                new KeyValuePair<string, string>("client_id", environment.ClientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
            ]),
        };

        using var response = await _httpClientFactory
            .CreateClient(IfsHttpClientNames.Authentication)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.AuthenticationFailed, null, "ifs_token_request_denied");
        }

        if (!response.IsSuccessStatusCode)
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.ProviderUnavailable, null, "ifs_token_request_failed");
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var payload = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        if (!payload.RootElement.TryGetProperty("access_token", out var accessTokenElement) ||
            accessTokenElement.ValueKind is not JsonValueKind.String ||
            string.IsNullOrWhiteSpace(accessTokenElement.GetString()))
        {
            return new IfsAccessTokenResult(IfsAccessTokenStatus.ProviderUnavailable, null, "ifs_token_response_invalid");
        }

        var expiresInSeconds = payload.RootElement.TryGetProperty("expires_in", out var expiresInElement) &&
            expiresInElement.TryGetInt32(out var parsedExpiresInSeconds)
            ? Math.Clamp(parsedExpiresInSeconds, 60, 3600)
            : 300;
        var accessToken = accessTokenElement.GetString()!;
        Cache[environment.Id] = new IfsCachedAccessToken(
            accessToken,
            _timeProvider.GetUtcNow().AddSeconds(expiresInSeconds));

        return new IfsAccessTokenResult(IfsAccessTokenStatus.Success, accessToken, null);
    }
}