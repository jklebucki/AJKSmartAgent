namespace Praxiara.Browser;

public sealed class ElementReferenceRegistry<TLocator> where TLocator : class
{
    private readonly Dictionary<string, TLocator> _locators = new(StringComparer.Ordinal);
    private readonly Lock _lock = new();

    public long Revision { get; private set; }

    public void Replace(long revision, IReadOnlyDictionary<string, TLocator> locators)
    {
        lock (_lock)
        {
            _locators.Clear();
            foreach (var item in locators)
            {
                _locators.Add(item.Key, item.Value);
            }

            Revision = revision;
        }
    }

    public TLocator Resolve(string elementReference, long expectedRevision)
    {
        lock (_lock)
        {
            if (Revision != expectedRevision)
            {
                throw new StalePageObservationException(expectedRevision, Revision);
            }

            return _locators.TryGetValue(elementReference, out var locator)
                ? locator
                : throw new UnknownElementReferenceException(elementReference);
        }
    }
}