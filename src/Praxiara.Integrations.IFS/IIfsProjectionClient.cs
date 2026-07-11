using System.Text.Json;

namespace Praxiara.Integrations.IFS;

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