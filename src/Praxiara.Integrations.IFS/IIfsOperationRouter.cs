namespace Praxiara.Integrations.IFS;

public interface IIfsOperationRouter
{
    ValueTask<IfsExecutionRoute> SelectRouteAsync(
        string operationName,
        IfsEnvironmentProfile environment,
        CancellationToken cancellationToken);
}