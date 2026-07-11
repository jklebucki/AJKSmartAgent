namespace Praxiara.Browser;

public interface IBrowserSessionFactory
{
    ValueTask<IBrowserSession> CreateAsync(Guid sessionId, CancellationToken cancellationToken);
}