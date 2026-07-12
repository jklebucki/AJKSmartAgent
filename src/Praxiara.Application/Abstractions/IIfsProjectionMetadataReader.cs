using Praxiara.Application.Ifs;

namespace Praxiara.Application.Abstractions;

public interface IIfsProjectionMetadataReader
{
    ValueTask<IfsMetadataReadResult> ReadAsync(
        string environmentId,
        string projectionName,
        CancellationToken cancellationToken);
}