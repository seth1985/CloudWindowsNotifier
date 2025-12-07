using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Api.Auth;

public class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(PortalUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.UserPrincipalName))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.UserPrincipalName));
        }
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim("name", user.DisplayName));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
}
