using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;

namespace Praxiara.Api.Security;

public sealed class KeycloakRoleClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        var realmAccess = principal.FindFirst("realm_access")?.Value;
        if (identity is null || string.IsNullOrWhiteSpace(realmAccess))
        {
            return Task.FromResult(principal);
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccess);
            if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind is not JsonValueKind.Array)
            {
                return Task.FromResult(principal);
            }

            foreach (var role in roles.EnumerateArray())
            {
                var roleName = role.GetString();
                if (!string.IsNullOrWhiteSpace(roleName) && !principal.IsInRole(roleName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
        catch (JsonException)
        {
            return Task.FromResult(principal);
        }

        return Task.FromResult(principal);
    }
}