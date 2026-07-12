using System.Security.Claims;

using Praxiara.Api.Security;

namespace Praxiara.IntegrationTests;

public sealed class KeycloakRoleClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsyncMapsRealmRolesToStandardRoleClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("realm_access", "{\"roles\":[\"praxiara-admin\",\"praxiara-operator\"]}")],
            "test"));

        var result = await new KeycloakRoleClaimsTransformation().TransformAsync(principal);

        Assert.True(result.IsInRole("praxiara-admin"));
        Assert.True(result.IsInRole("praxiara-operator"));
    }

    [Fact]
    public async Task TransformAsyncIgnoresMalformedRealmRoleClaim()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("realm_access", "not-json")],
            "test"));

        var result = await new KeycloakRoleClaimsTransformation().TransformAsync(principal);

        Assert.False(result.IsInRole("praxiara-admin"));
    }
}