using System.Net;
using System.Text.RegularExpressions;

using Praxiara.Application.Abstractions;
using Praxiara.Application.Ifs;

namespace Praxiara.Integrations.IFS;

public sealed class IfsProjectionMetadataReader(
    IIfsEnvironmentConfigurationStore environmentStore,
    IIfsAccessTokenProvider accessTokenProvider,
    IHttpClientFactory httpClientFactory) : IIfsProjectionMetadataReader
{
    private const int MaximumMetadataBytes = 1_048_576;
    private static readonly Regex ProjectionNameRegex = new("^[A-Za-z][A-Za-z0-9]{0,127}$", RegexOptions.CultureInvariant);
    private readonly IIfsEnvironmentConfigurationStore _environmentStore = environmentStore;
    private readonly IIfsAccessTokenProvider _accessTokenProvider = accessTokenProvider;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    public async ValueTask<IfsMetadataReadResult> ReadAsync(
        string environmentId,
        string projectionName,
        CancellationToken cancellationToken)
    {
        if (!ProjectionNameRegex.IsMatch(projectionName))
        {
            return Failure(IfsMetadataReadStatus.ProjectionNotAllowed, "ifs_projection_not_allowed");
        }

        IfsEnvironmentConfiguration? environment;
        try
        {
            environment = await _environmentStore.FindAsync(environmentId, cancellationToken);
        }
        catch (IfsEnvironmentStorageUnavailableException)
        {
            return Failure(IfsMetadataReadStatus.StorageUnavailable, "ifs_environment_storage_unavailable");
        }

        if (environment is null)
        {
            return Failure(IfsMetadataReadStatus.EnvironmentNotFound, "ifs_environment_not_found");
        }

        if (!environment.AllowedProjectionNames.Contains(projectionName))
        {
            return Failure(IfsMetadataReadStatus.ProjectionNotAllowed, "ifs_projection_not_allowed");
        }

        var tokenResult = await _accessTokenProvider.GetAccessTokenAsync(environment, cancellationToken);
        if (tokenResult.Status is not IfsAccessTokenStatus.Success || string.IsNullOrWhiteSpace(tokenResult.AccessToken))
        {
            return MapTokenFailure(tokenResult);
        }

        var relativePath = $"/main/ifsapplications/projection/v1/{projectionName}.svc/$metadata";
        var metadataUri = new Uri(environment.BaseUri, relativePath);
        using var request = new HttpRequestMessage(HttpMethod.Get, metadataUri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
        request.Headers.Accept.ParseAdd("application/xml, application/json;q=0.5");

        using var response = await _httpClientFactory
            .CreateClient(IfsHttpClientNames.ProjectionMetadata)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return Failure(IfsMetadataReadStatus.AuthenticationFailed, "ifs_metadata_access_denied");
        }

        if (!response.IsSuccessStatusCode)
        {
            return Failure(IfsMetadataReadStatus.ProviderUnavailable, "ifs_metadata_request_failed");
        }

        var contentLength = response.Content.Headers.ContentLength;
        if (contentLength is > MaximumMetadataBytes)
        {
            return Failure(IfsMetadataReadStatus.InvalidResponse, "ifs_metadata_response_too_large");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, leaveOpen: false);
        var content = await reader.ReadToEndAsync(cancellationToken);
        if (content.Length is 0 or > MaximumMetadataBytes || !LooksLikeMetadata(content))
        {
            return Failure(IfsMetadataReadStatus.InvalidResponse, "ifs_metadata_response_invalid");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/xml";
        return new IfsMetadataReadResult(IfsMetadataReadStatus.Success, content, contentType, null);
    }

    private static bool LooksLikeMetadata(string content) =>
        content.Contains("edmx:Edmx", StringComparison.Ordinal) ||
        content.Contains("<Edmx", StringComparison.Ordinal) ||
        content.Contains("@odata.context", StringComparison.Ordinal);

    private static IfsMetadataReadResult MapTokenFailure(IfsAccessTokenResult result) =>
        result.Status switch
        {
            IfsAccessTokenStatus.CredentialsUnavailable => Failure(IfsMetadataReadStatus.CredentialsUnavailable, result.ErrorCode),
            IfsAccessTokenStatus.AuthenticationFailed => Failure(IfsMetadataReadStatus.AuthenticationFailed, result.ErrorCode),
            _ => Failure(IfsMetadataReadStatus.ProviderUnavailable, result.ErrorCode),
        };

    private static IfsMetadataReadResult Failure(IfsMetadataReadStatus status, string? errorCode) =>
        new(status, null, null, errorCode);

}