using Praxiara.Browser;

namespace Praxiara.Browser.Tests;

public sealed class ElementReferenceRegistryTests
{
    [Fact]
    public void ResolveRejectsReferenceFromStaleRevision()
    {
        var registry = new ElementReferenceRegistry<object>();
        registry.Replace(12, new Dictionary<string, object> { ["e1"] = new() });

        var exception = Assert.Throws<StalePageObservationException>(() => registry.Resolve("e1", 11));

        Assert.Equal(11, exception.Expected);
        Assert.Equal(12, exception.Current);
    }

    [Fact]
    public void ReplaceInvalidatesPreviousReferences()
    {
        var registry = new ElementReferenceRegistry<object>();
        registry.Replace(1, new Dictionary<string, object> { ["e1"] = new() });
        registry.Replace(2, new Dictionary<string, object> { ["e2"] = new() });

        Assert.Throws<UnknownElementReferenceException>(() => registry.Resolve("e1", 2));
    }
}