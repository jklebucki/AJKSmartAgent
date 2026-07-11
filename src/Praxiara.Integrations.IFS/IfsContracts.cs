using System.Text.Json;

namespace Praxiara.Integrations.IFS;

public sealed record IfsEnvironmentProfile(
    string Id,
    Uri BaseUri,
    string Tenant,
    string Locale,
    string EnvironmentKind,
    IReadOnlySet<string> AllowedProjectionNames);

public interface IIfsProjectionClient
{
    ValueTask<JsonDocument> QueryAsync(
        IfsEnvironmentProfile environment,
        string projection,
        string relativePath,
        CancellationToken cancellationToken);

    ValueTask<JsonDocument> InvokeActionAsync(
        IfsEnvironmentProfile environment,
        string projection,
        string action,
        JsonElement input,
        CancellationToken cancellationToken);
}

public interface IIfsOperationRouter
{
    ValueTask<IfsExecutionRoute> SelectRouteAsync(
        string operationName,
        IfsEnvironmentProfile environment,
        CancellationToken cancellationToken);
}

public enum IfsExecutionRoute
{
    ProjectionApi,
    Browser,
    Hybrid
}