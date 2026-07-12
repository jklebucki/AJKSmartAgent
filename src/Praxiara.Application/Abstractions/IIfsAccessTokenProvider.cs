using Praxiara.Application.Ifs;

namespace Praxiara.Application.Abstractions;

public interface IIfsAccessTokenProvider
{
    ValueTask<IfsAccessTokenResult> GetAccessTokenAsync(
        IfsEnvironmentConfiguration environment,
        CancellationToken cancellationToken);
}