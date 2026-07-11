using Microsoft.Extensions.AI;

namespace Praxiara.Llm;

public interface IChatClientResolver
{
    IChatClient Resolve(ModelCapability capability);
}