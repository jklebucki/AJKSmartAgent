using Microsoft.Extensions.AI;

namespace Praxiara.Llm;

public interface IChatClientResolver
{
    IChatClient Resolve(ModelCapability capability);
}

public enum ModelCapability
{
    Planning,
    Vision,
    RiskClassification,
    Summarization
}

public sealed record ModelRoute(
    ModelCapability Capability,
    string Provider,
    string ModelId,
    int MaximumOutputTokens,
    TimeSpan Timeout);