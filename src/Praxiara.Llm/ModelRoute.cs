namespace Praxiara.Llm;

public sealed record ModelRoute(
    ModelCapability Capability,
    string Provider,
    string ModelId,
    int MaximumOutputTokens,
    TimeSpan Timeout);