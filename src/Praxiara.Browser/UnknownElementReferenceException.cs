namespace Praxiara.Browser;

public sealed class UnknownElementReferenceException(string elementReference)
    : KeyNotFoundException($"Element reference '{elementReference}' does not exist in the current observation.")
{
    public string ElementReference { get; } = elementReference;
}