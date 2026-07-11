using Praxiara.Contracts.Browser;

namespace Praxiara.Application.Abstractions;

public interface IObservationProvider
{
    ValueTask<BrowserObservation> ObserveAsync(Guid sessionId, CancellationToken cancellationToken);
}