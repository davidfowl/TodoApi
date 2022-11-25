using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddTokenService(this IServiceCollection services)
    {
        return services.AddSingleton<ITokenService, TokenService>();
    }
}

public interface ITokenService
{
    string GenerateToken(string username, bool isAdmin = false);
}

public class TokenService : ITokenService
{
    private readonly string _issuer;
    private readonly SigningCredentials _jwtSigningCredentials;
    private readonly string[] _audiences;

    public TokenService(IAuthenticationConfigurationProvider authenticationConfigurationProvider)
    {
        var bearerSection = authenticationConfigurationProvider.GetSchemeConfiguration(JwtBearerDefaults.AuthenticationScheme);

        var section = bearerSection.GetSection("SigningKeys:0");

        _issuer = section["Issuer"] ?? throw new InvalidOperationException("Issuer is not specifed");
        var signingKeyBase64 = section["Value"] ?? throw new InvalidOperationException("Signing key is not specified");

        _jwtSigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(signingKeyBase64)), 
            SecurityAlgorithms.HmacSha256Signature);

        _audiences = bearerSection.GetSection("ValidAudiences").GetChildren()
                    .Where(s => !string.IsNullOrEmpty(s.Value))
                    .Select(s => s.Value!)
                    .ToArray();
    }

    public string GenerateToken(string username, bool isAdmin = false)
    {
        var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub, username));

        // REVIEW: Check that this logic is OK for jti claims
        var id = Guid.NewGuid().ToString().GetHashCode().ToString("x", CultureInfo.InvariantCulture);

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Jti, id));

        if (isAdmin)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
        }

        identity.AddClaims(_audiences.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)));

        var handler = new JwtSecurityTokenHandler();

        var jwtToken = handler.CreateJwtSecurityToken(
            _issuer,
            audience: null,
            identity,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(30),
            issuedAt: DateTime.UtcNow,
            _jwtSigningCredentials);

        return handler.WriteToken(jwtToken);
    }
}
