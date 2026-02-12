using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WindowsNotifierCloud.Api.Auth;
using WindowsNotifierCloud.Api.Models.Auth;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPortalUserRepository _users;
    private readonly JwtTokenService _jwt;
    private readonly AuthenticationOptions _authOptions;

    public AuthController(
        IPortalUserRepository users,
        IOptions<JwtOptions> jwtOptions,
        AuthenticationOptions authOptions)
    {
        _users = users;
        _jwt = new JwtTokenService(jwtOptions.Value);
        _authOptions = authOptions;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!string.Equals(_authOptions.Provider, "Local", StringComparison.OrdinalIgnoreCase))
        {
            return Forbid(); // Local username/password is disabled when Entra provider is active.
        }

        var user = await _users.GetByLocalUsernameAsync(request.Username, ct);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash) || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            await Task.Delay(100, ct); // small delay to reduce user enumeration
            return Unauthorized();
        }

        var (token, expires) = _jwt.GenerateToken(user);
        return new LoginResponse
        {
            Token = token,
            Role = user.Role.ToString(),
            ExpiresAtUtc = expires
        };
    }
}
