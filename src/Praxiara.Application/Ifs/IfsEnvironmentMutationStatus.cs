namespace Praxiara.Application.Ifs;

public enum IfsEnvironmentMutationStatus
{
    Success,
    NotFound,
    AlreadyExists,
    ValidationFailed,
    StorageUnavailable,
}