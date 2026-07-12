using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Praxiara.Api.Security;

public static class PraxiaraAuthenticationExtensions
{
    public const string IdentityScheme = "PraxiaraIdentity";

    public static void AddPraxiaraAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(PraxiaraAuthenticationOptions.SectionName).Get<PraxiaraAuthenticationOptions>()
            ?? new PraxiaraAuthenticationOptions();
        services.AddSingleton(options);

        if (!options.IsConfigured)
        {
            services
                .AddAuthentication(DenyAllAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, DenyAllAuthenticationHandler>(
                    DenyAllAuthenticationHandler.SchemeName,
                    _ => { });
            return;
        }

        services.AddTransient<IClaimsTransformation, KeycloakRoleClaimsTransformation>();
        services.AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = IdentityScheme;
                authenticationOptions.DefaultChallengeScheme = IdentityScheme;
                authenticationOptions.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddPolicyScheme(IdentityScheme, IdentityScheme, policyOptions =>
            {
                policyOptions.ForwardDefaultSelector = context =>
                    context.Request.Headers.Authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, cookieOptions =>
            {
                cookieOptions.Cookie.Name = "praxiara-session";
                cookieOptions.Cookie.HttpOnly = true;
                cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                cookieOptions.SlidingExpiration = false;
                cookieOptions.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, openIdConnectOptions =>
            {
                openIdConnectOptions.Authority = options.Authority;
                openIdConnectOptions.ClientId = options.ClientId;
                openIdConnectOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
                openIdConnectOptions.ResponseType = "code";
                openIdConnectOptions.UsePkce = true;
                openIdConnectOptions.SaveTokens = false;
                openIdConnectOptions.GetClaimsFromUserInfoEndpoint = false;
                openIdConnectOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                openIdConnectOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
            {
                jwtBearerOptions.Authority = options.Authority;
                jwtBearerOptions.Audience = options.Audience;
                jwtBearerOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds),
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                };
            });
    }
}