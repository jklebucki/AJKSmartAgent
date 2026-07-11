namespace Praxiara.Browser;

public sealed class StalePageObservationException(long expected, long current)
    : InvalidOperationException($"Page revision '{expected}' is stale; current revision is '{current}'.")
{
    public long Expected { get; } = expected;

    public long Current { get; } = current;
}