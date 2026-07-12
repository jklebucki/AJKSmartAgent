namespace Praxiara.Application.Ifs;

public sealed class IfsEnvironmentStorageUnavailableException(string message) : InvalidOperationException(message);