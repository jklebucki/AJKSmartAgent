using System.Text.RegularExpressions;

using Praxiara.Application.Abstractions;
using Praxiara.Contracts.Ifs;

namespace Praxiara.Application.Ifs;

public sealed class IfsEnvironmentAdministrationService(IIfsEnvironmentConfigurationStore store)
{
    private static readonly Regex EnvironmentIdRegex = new("^[a-z][a-z0-9-]{1,62}$", RegexOptions.CultureInvariant);
    private static readonly Regex EnvironmentKindRegex = new("^(Development|Test|Acceptance|Production)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex ProjectionNameRegex = new("^[A-Za-z][A-Za-z0-9]{0,127}$", RegexOptions.CultureInvariant);
    private readonly IIfsEnvironmentConfigurationStore _store = store;

    public ValueTask<IReadOnlyList<IfsEnvironmentConfiguration>> ListAsync(CancellationToken cancellationToken) =>
        _store.ListAsync(cancellationToken);

    public ValueTask<IfsEnvironmentConfiguration?> FindAsync(string id, CancellationToken cancellationToken) =>
        _store.FindAsync(id, cancellationToken);

    public async ValueTask<IfsEnvironmentMutationResult> CreateAsync(
        IfsEnvironmentCreateRequest request,
        string actorId,
        CancellationToken cancellationToken)
    {
        var result = TryCreateConfiguration(
            request.Id,
            request.BaseUri,
            request.Tenant,
            request.Locale,
            request.EnvironmentKind,
            request.AllowedProjectionNames,
            request.AuthenticationMode,
            request.SecretFilePath,
            request.TokenEndpoint,
            request.ClientId);

        if (result.Configuration is null)
        {
            return result;
        }

        if (await _store.FindAsync(result.Configuration.Id, cancellationToken) is not null)
        {
            return new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.AlreadyExists, null, "ifs_environment_already_exists");
        }

        try
        {
            await _store.CreateAsync(result.Configuration, actorId, cancellationToken);
            return result;
        }
        catch (IfsEnvironmentStorageUnavailableException)
        {
            return new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.StorageUnavailable, null, "ifs_environment_storage_unavailable");
        }
    }

    public async ValueTask<IfsEnvironmentMutationResult> UpdateAsync(
        string id,
        IfsEnvironmentUpdateRequest request,
        string actorId,
        CancellationToken cancellationToken)
    {
        var existing = await _store.FindAsync(id, cancellationToken);
        if (existing is null)
        {
            return new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.NotFound, null, "ifs_environment_not_found");
        }

        var result = TryCreateConfiguration(
            id,
            request.BaseUri,
            request.Tenant,
            request.Locale,
            request.EnvironmentKind,
            request.AllowedProjectionNames,
            request.AuthenticationMode,
            request.SecretFilePath ?? existing.SecretFilePath,
            request.TokenEndpoint,
            request.ClientId);

        if (result.Configuration is null)
        {
            return result;
        }

        try
        {
            var updated = await _store.UpdateAsync(result.Configuration, actorId, cancellationToken);
            return updated
                ? result
                : new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.NotFound, null, "ifs_environment_not_found");
        }
        catch (IfsEnvironmentStorageUnavailableException)
        {
            return new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.StorageUnavailable, null, "ifs_environment_storage_unavailable");
        }
    }

    public async ValueTask<IfsEnvironmentMutationResult> DeleteAsync(
        string id,
        string actorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _store.DeleteAsync(id, actorId, cancellationToken);
            return deleted
                ? new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.Success, null, null)
                : new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.NotFound, null, "ifs_environment_not_found");
        }
        catch (IfsEnvironmentStorageUnavailableException)
        {
            return new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.StorageUnavailable, null, "ifs_environment_storage_unavailable");
        }
    }

    private static IfsEnvironmentMutationResult TryCreateConfiguration(
        string id,
        string baseUriText,
        string tenant,
        string locale,
        string environmentKind,
        IReadOnlyList<string> allowedProjectionNames,
        string authenticationModeText,
        string secretFilePath,
        string? tokenEndpointText,
        string? clientId)
    {
        if (!EnvironmentIdRegex.IsMatch(id))
        {
            return ValidationFailed("ifs_environment_id_invalid");
        }

        if (!TryParseBaseUri(baseUriText, out var baseUri))
        {
            return ValidationFailed("ifs_environment_base_uri_invalid");
        }

        if (string.IsNullOrWhiteSpace(tenant) || tenant.Length > 128 ||
            string.IsNullOrWhiteSpace(locale) || locale.Length > 32 ||
            !EnvironmentKindRegex.IsMatch(environmentKind))
        {
            return ValidationFailed("ifs_environment_scope_invalid");
        }

        if (!TryParseAllowedProjectionNames(allowedProjectionNames, out var projections))
        {
            return ValidationFailed("ifs_environment_projection_allowlist_invalid");
        }

        if (!Enum.TryParse<IfsAuthenticationMode>(authenticationModeText, true, out var authenticationMode) ||
            !IsSecretFilePathValid(secretFilePath))
        {
            return ValidationFailed("ifs_environment_authentication_invalid");
        }

        Uri? tokenEndpoint = null;
        if (authenticationMode is IfsAuthenticationMode.ClientCredentials)
        {
            if (string.IsNullOrWhiteSpace(clientId) || clientId.Length > 256 ||
                !TryParseTokenEndpoint(tokenEndpointText, baseUri, out tokenEndpoint))
            {
                return ValidationFailed("ifs_environment_client_credentials_invalid");
            }
        }
        else if (!string.IsNullOrWhiteSpace(tokenEndpointText) || !string.IsNullOrWhiteSpace(clientId))
        {
            return ValidationFailed("ifs_environment_bearer_token_file_invalid");
        }

        var configuration = new IfsEnvironmentConfiguration(
            id.Trim(),
            baseUri,
            tenant.Trim(),
            locale.Trim(),
            environmentKind.Trim(),
            projections,
            authenticationMode,
            secretFilePath.Trim(),
            tokenEndpoint,
            clientId?.Trim());

        return new IfsEnvironmentMutationResult(IfsEnvironmentMutationStatus.Success, configuration, null);
    }

    private static bool TryParseBaseUri(string value, out Uri baseUri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed) &&
            string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(parsed.UserInfo) &&
            string.IsNullOrEmpty(parsed.Query) &&
            string.IsNullOrEmpty(parsed.Fragment) &&
            (parsed.AbsolutePath == "/" || string.IsNullOrEmpty(parsed.AbsolutePath)))
        {
            baseUri = new Uri(parsed.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
            return true;
        }

        baseUri = null!;
        return false;
    }

    private static bool TryParseTokenEndpoint(string? value, Uri baseUri, out Uri? tokenEndpoint)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed) &&
            string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(parsed.Scheme, baseUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(parsed.Host, baseUri.Host, StringComparison.OrdinalIgnoreCase) &&
            parsed.Port == baseUri.Port &&
            string.IsNullOrEmpty(parsed.UserInfo) &&
            string.IsNullOrEmpty(parsed.Fragment))
        {
            tokenEndpoint = parsed;
            return true;
        }

        tokenEndpoint = null;
        return false;
    }

    private static bool TryParseAllowedProjectionNames(
        IReadOnlyList<string> source,
        out IReadOnlySet<string> projections)
    {
        var values = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in source)
        {
            if (!ProjectionNameRegex.IsMatch(value))
            {
                projections = null!;
                return false;
            }

            values.Add(value);
        }

        projections = values;
        return values.Count > 0 && values.Count <= 200;
    }

    private static bool IsSecretFilePathValid(string value) =>
        Path.IsPathFullyQualified(value) && !value.Contains("..", StringComparison.Ordinal);

    private static IfsEnvironmentMutationResult ValidationFailed(string errorCode) =>
        new(IfsEnvironmentMutationStatus.ValidationFailed, null, errorCode);

}