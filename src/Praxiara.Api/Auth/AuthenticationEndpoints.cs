using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Praxiara.Api.Auth;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth").WithTags("Authentication");

        group.MapGet("/login", Login).WithName("Login").AllowAnonymous();
        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute(true));
        group.MapGet("/antiforgery", GetAntiforgeryToken).WithName("GetAntiforgeryToken").RequireAuthorization();

        return endpoints;
    }

    private static ChallengeHttpResult Login(string? returnUrl) =>
        TypedResults.Challenge(
            new AuthenticationProperties { RedirectUri = IsLocalReturnUrl(returnUrl) ? returnUrl : "/" },
            [OpenIdConnectDefaults.AuthenticationScheme]);

    private static SignOutHttpResult Logout() =>
        TypedResults.SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);

    private static Ok<AntiforgeryTokenResponse> GetAntiforgeryToken(IAntiforgery antiforgery, HttpContext context) =>
        TypedResults.Ok(new AntiforgeryTokenResponse(antiforgery.GetAndStoreTokens(context).RequestToken!));

    private static bool IsLocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) &&
        returnUrl.StartsWith('/') &&
        !returnUrl.StartsWith("//", StringComparison.Ordinal);
}