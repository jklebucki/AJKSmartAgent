using Praxiara.Application.Abstractions;
using Praxiara.Application.Ifs;

namespace Praxiara.Api.Ifs;

public sealed class UnconfiguredIfsEnvironmentConfigurationStore : IIfsEnvironmentConfigurationStore
{
    public ValueTask<IReadOnlyList<IfsEnvironmentConfiguration>> ListAsync(CancellationToken cancellationToken) =>
        ValueTask.FromException<IReadOnlyList<IfsEnvironmentConfiguration>>(CreateException());

    public ValueTask<IfsEnvironmentConfiguration?> FindAsync(string id, CancellationToken cancellationToken) =>
        ValueTask.FromException<IfsEnvironmentConfiguration?>(CreateException());

    public ValueTask CreateAsync(IfsEnvironmentConfiguration configuration, string actorId, CancellationToken cancellationToken) =>
        ValueTask.FromException(CreateException());

    public ValueTask<bool> UpdateAsync(IfsEnvironmentConfiguration configuration, string actorId, CancellationToken cancellationToken) =>
        ValueTask.FromException<bool>(CreateException());

    public ValueTask<bool> DeleteAsync(string id, string actorId, CancellationToken cancellationToken) =>
        ValueTask.FromException<bool>(CreateException());

    private static IfsEnvironmentStorageUnavailableException CreateException() =>
        new("IFS environment persistence is not configured.");
}