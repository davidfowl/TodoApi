using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi.Tests;

// Lifted from: https://github.com/dotnet/aspnetcore/blob/b39a258cbce1b16ee98679ef7d2ddc2e09040a6b/src/Tools/dotnet-user-jwts/src/Helpers/JwtIssuer.cs#L13
internal sealed class JwtIssuer
{
    private readonly SymmetricSecurityKey _signingKey;

    public JwtIssuer(string issuer, byte[] signingKeyMaterial)
    {
        Issuer = issuer;
        _signingKey = new SymmetricSecurityKey(signingKeyMaterial);
    }

    public string Issuer { get; }

    public JwtSecurityToken Create(JwtCreatorOptions options)
    {
        var identity = new GenericIdentity(options.Name);

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, options.Name));

        var id = Guid.NewGuid().ToString().GetHashCode().ToString("x", CultureInfo.InvariantCulture);
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, id));

        if (options.Scopes is { } scopesToAdd)
        {
            identity.AddClaims(scopesToAdd.Select(s => new Claim("scope", s)));
        }

        if (options.Roles is { } rolesToAdd)
        {
            identity.AddClaims(rolesToAdd.Select(r => new Claim(ClaimTypes.Role, r)));
        }

        if (options.Claims is { Count: > 0 } claimsToAdd)
        {
            identity.AddClaims(claimsToAdd);
        }

        // Although the JwtPayload supports having multiple audiences registered, the
        // creator methods and constructors don't provide a way of setting multiple
        // audiences. Instead, we have to register an `aud` claim for each audience
        // we want to add so that the multiple audiences are populated correctly.
        if (options.Audiences is { Count: > 0 } audiences)
        {
            identity.AddClaims(audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)));
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtSigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature);
        var jwtToken = handler.CreateJwtSecurityToken(Issuer, audience: null, identity, options.NotBefore, options.ExpiresOn, issuedAt: DateTime.UtcNow, jwtSigningCredentials);
        return jwtToken;
    }

    public static string WriteToken(JwtSecurityToken token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(token);
    }

    public static string CreateToken(IConfiguration configuration, string id, bool isAdmin)
    {
        List<string>? roles = null;

        if (isAdmin)
        {
            roles = new()
            {
                "admin"
            };
        }
        var claims = new List<Claim>
        {
            new("id", id)
        };

        return CreateToken(configuration, claims, roles);
    }

    public static string CreateToken(IConfiguration configuration,
                                     IList<Claim>? claims = null,
                                     IList<string>? roles = null,
                                     IList<string>? scopes = null)
    {
        // Read the user JWTs configuration for testing so unit tests can generate
        // JWT tokens.

        var bearerSection = configuration.GetSection("Authentication:Schemes:Bearer");
        var section = bearerSection.GetSection("SigningKeys:0");
        var issuer = section["Issuer"] ?? throw new InvalidOperationException("Missing issuer");
        var signingKeyBase64 = section["Value"] ?? throw new InvalidOperationException("Missing signing key");

        var signingKeyBytes = Convert.FromBase64String(signingKeyBase64);

        var audiences = bearerSection.GetSection("ValidAudiences")
                                     .GetChildren()
                                     .Where(s => !string.IsNullOrEmpty(s.Value))
                                     .Select(s => s.Value!)
                                     .ToList();

        var jwtIssuer = new JwtIssuer(issuer, signingKeyBytes);

        var token = jwtIssuer.Create(new(
            JwtBearerDefaults.AuthenticationScheme,
            Name: Guid.NewGuid().ToString(),
            Audiences: audiences,
            Issuer: jwtIssuer.Issuer,
            NotBefore: DateTime.UtcNow,
            ExpiresOn: DateTime.UtcNow.AddDays(1),
            Roles: roles,
            Scopes: scopes,
            Claims: claims));

        return WriteToken(token);
    }
}

internal sealed record JwtCreatorOptions(
    string Scheme,
    string Name,
    IList<string>? Audiences,
    string Issuer,
    DateTime NotBefore,
    DateTime ExpiresOn,
    IList<string>? Roles,
    IList<string>? Scopes,
    IList<Claim>? Claims);