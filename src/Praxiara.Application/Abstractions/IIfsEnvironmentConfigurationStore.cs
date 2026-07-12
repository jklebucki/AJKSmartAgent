using Praxiara.Application.Ifs;

namespace Praxiara.Application.Abstractions;

public interface IIfsEnvironmentConfigurationStore
{
    ValueTask<IReadOnlyList<IfsEnvironmentConfiguration>> ListAsync(CancellationToken cancellationToken);

    ValueTask<IfsEnvironmentConfiguration?> FindAsync(string id, CancellationToken cancellationToken);

    ValueTask CreateAsync(IfsEnvironmentConfiguration configuration, string actorId, CancellationToken cancellationToken);

    ValueTask<bool> UpdateAsync(IfsEnvironmentConfiguration configuration, string actorId, CancellationToken cancellationToken);

    ValueTask<bool> DeleteAsync(string id, string actorId, CancellationToken cancellationToken);
}