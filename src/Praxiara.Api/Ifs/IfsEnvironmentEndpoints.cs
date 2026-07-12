using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Praxiara.Api.Security;
using Praxiara.Application.Abstractions;
using Praxiara.Application.Ifs;
using Praxiara.Contracts.Ifs;

namespace Praxiara.Api.Ifs;

public static class IfsEnvironmentEndpoints
{
    public static IEndpointRouteBuilder MapIfsEnvironmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/ifs/environments")
            .WithTags("IFS environments");

        group.MapGet("", ListAsync)
            .WithName("ListIfsEnvironments")
            .RequireAuthorization(IfsAuthorizationPolicies.EnvironmentRead);
        group.MapPost("", CreateAsync)
            .WithName("CreateIfsEnvironment")
            .RequireAuthorization(IfsAuthorizationPolicies.EnvironmentWrite);
        group.MapPut("/{environmentId}", UpdateAsync)
            .WithName("UpdateIfsEnvironment")
            .RequireAuthorization(IfsAuthorizationPolicies.EnvironmentWrite);
        group.MapDelete("/{environmentId}", DeleteAsync)
            .WithName("DeleteIfsEnvironment")
            .RequireAuthorization(IfsAuthorizationPolicies.EnvironmentWrite);
        group.MapGet("/{environmentId}/projections/{projectionName}/metadata", ReadMetadataAsync)
            .WithName("GetIfsProjectionMetadata")
            .RequireAuthorization(IfsAuthorizationPolicies.EnvironmentRead);

        return endpoints;
    }

    private static async Task<Results<Ok<IReadOnlyList<IfsEnvironmentResponse>>, ProblemHttpResult>> ListAsync(
        IfsEnvironmentAdministrationService service,
        CancellationToken cancellationToken)
    {
        try
        {
            var configurations = await service.ListAsync(cancellationToken);
            return TypedResults.Ok<IReadOnlyList<IfsEnvironmentResponse>>(
                configurations.Select(ToResponse).ToArray());
        }
        catch (IfsEnvironmentStorageUnavailableException)
        {
            return StorageUnavailable();
        }
    }

    private static async Task<Results<Created<IfsEnvironmentResponse>, Conflict<ProblemDetails>, BadRequest<ProblemDetails>, ProblemHttpResult>> CreateAsync(
        IfsEnvironmentCreateRequest request,
        ClaimsPrincipal user,
        IfsEnvironmentAdministrationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, GetActorId(user), cancellationToken);
        return result.Status switch
        {
            IfsEnvironmentMutationStatus.Success => TypedResults.Created($"/api/v1/ifs/environments/{result.Configuration!.Id}", ToResponse(result.Configuration)),
            IfsEnvironmentMutationStatus.AlreadyExists => TypedResults.Conflict(CreateProblem(result.ErrorCode!, StatusCodes.Status409Conflict)),
            IfsEnvironmentMutationStatus.ValidationFailed => TypedResults.BadRequest(CreateProblem(result.ErrorCode!, StatusCodes.Status400BadRequest)),
            _ => StorageUnavailable(),
        };
    }

    private static async Task<Results<Ok<IfsEnvironmentResponse>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>, ProblemHttpResult>> UpdateAsync(
        string environmentId,
        IfsEnvironmentUpdateRequest request,
        ClaimsPrincipal user,
        IfsEnvironmentAdministrationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(environmentId, request, GetActorId(user), cancellationToken);
        return result.Status switch
        {
            IfsEnvironmentMutationStatus.Success => TypedResults.Ok(ToResponse(result.Configuration!)),
            IfsEnvironmentMutationStatus.NotFound => TypedResults.NotFound(CreateProblem(result.ErrorCode!, StatusCodes.Status404NotFound)),
            IfsEnvironmentMutationStatus.ValidationFailed => TypedResults.BadRequest(CreateProblem(result.ErrorCode!, StatusCodes.Status400BadRequest)),
            _ => StorageUnavailable(),
        };
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>, ProblemHttpResult>> DeleteAsync(
        string environmentId,
        ClaimsPrincipal user,
        IfsEnvironmentAdministrationService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(environmentId, GetActorId(user), cancellationToken);
        return result.Status switch
        {
            IfsEnvironmentMutationStatus.Success => TypedResults.NoContent(),
            IfsEnvironmentMutationStatus.NotFound => TypedResults.NotFound(CreateProblem(result.ErrorCode!, StatusCodes.Status404NotFound)),
            _ => StorageUnavailable(),
        };
    }

    private static async Task<IResult> ReadMetadataAsync(
        string environmentId,
        string projectionName,
        IIfsProjectionMetadataReader reader,
        CancellationToken cancellationToken)
    {
        var result = await reader.ReadAsync(environmentId, projectionName, cancellationToken);
        return result.Status switch
        {
            IfsMetadataReadStatus.Success => TypedResults.Content(
                result.Content!,
                result.ContentType ?? "application/xml",
                Encoding.UTF8,
                StatusCodes.Status200OK),
            IfsMetadataReadStatus.EnvironmentNotFound => TypedResults.NotFound(CreateProblem(result.ErrorCode!, StatusCodes.Status404NotFound)),
            IfsMetadataReadStatus.ProjectionNotAllowed => TypedResults.Forbid(),
            IfsMetadataReadStatus.StorageUnavailable => StorageUnavailable(),
            IfsMetadataReadStatus.CredentialsUnavailable => TypedResults.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "IFS credentials are unavailable.",
                extensions: new Dictionary<string, object?> { ["code"] = result.ErrorCode }),
            _ => TypedResults.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "IFS metadata request failed.",
                extensions: new Dictionary<string, object?> { ["code"] = result.ErrorCode }),
        };
    }

    private static IfsEnvironmentResponse ToResponse(IfsEnvironmentConfiguration configuration) =>
        new(
            configuration.Id,
            configuration.BaseUri.AbsoluteUri,
            configuration.Tenant,
            configuration.Locale,
            configuration.EnvironmentKind,
            configuration.AllowedProjectionNames.Order(StringComparer.Ordinal).ToArray(),
            configuration.AuthenticationMode.ToString(),
            configuration.TokenEndpoint?.AbsoluteUri,
            configuration.ClientId,
            !string.IsNullOrWhiteSpace(configuration.SecretFilePath));

    private static string GetActorId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) ??
        user.FindFirstValue("sub") ??
        user.Identity?.Name ??
        "unknown";

    private static ProblemDetails CreateProblem(string code, int statusCode) =>
        new()
        {
            Status = statusCode,
            Title = "IFS environment request failed.",
            Extensions = { ["code"] = code },
        };

    private static ProblemHttpResult StorageUnavailable() =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "IFS environment storage is unavailable.",
            extensions: new Dictionary<string, object?> { ["code"] = "ifs_environment_storage_unavailable" });
}