namespace Praxiara.Application.Ifs;

public enum IfsMetadataReadStatus
{
    Success,
    EnvironmentNotFound,
    ProjectionNotAllowed,
    StorageUnavailable,
    CredentialsUnavailable,
    AuthenticationFailed,
    ProviderUnavailable,
    InvalidResponse,
}