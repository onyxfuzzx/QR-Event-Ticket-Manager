using Dapper;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Enums;
using QREventPlatform.Advanced.Security;
using QREventPlatform.Advanced.Models;
using QREventPlatform.Advanced.DTOs;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DapperContext _ctx;
    private readonly TokenService _token;

    public AuthController(DapperContext ctx, TokenService token)
    {
        _ctx = ctx;
        _token = token;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest req)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        using var db = _ctx.CreateConnection();

        var user = db.QuerySingleOrDefault<LoginUserDto>(
                """
            SELECT Id, PasswordHash, Role, IsActive
            FROM Users
            WHERE LOWER(Email) = LOWER(@Email)
            """,
                new { req.Email }
            );

        if (user == null || !user.IsActive)
            return Unauthorized("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var role = (Role)user.Role;
        var token = _token.CreateToken(user.Id, role);

        return Ok(new
        {
            accessToken = token,
            tokenType = "Bearer",
            role = role.ToString()
        });
    }
}
