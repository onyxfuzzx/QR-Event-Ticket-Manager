using Dapper;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Enums;
using QREventPlatform.Advanced.Security;
using QREventPlatform.Advanced.Services;
using QREventPlatform.Advanced.Models;
using QREventPlatform.Advanced.DTOs;
using Microsoft.AspNetCore.Authorization;
using QREventPlatform.Advanced.Extensions;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DapperContext _ctx;
    private readonly TokenService _token;
    private readonly EmailService _email;
    private readonly IConfiguration _config;

    public AuthController(DapperContext ctx, TokenService token, EmailService email, IConfiguration config)
    {
        _ctx = ctx;
        _token = token;
        _email = email;
        _config = config;
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

    [Authorize]
    [HttpPost("change-password")]
    public IActionResult ChangePassword(ChangePasswordRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.GetUserId();
        using var db = _ctx.CreateConnection();

        var user = db.QuerySingleOrDefault<LoginUserDto>(
            "SELECT PasswordHash, IsActive FROM Users WHERE Id = @userId",
            new { userId });

        if (user == null || !user.IsActive) return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Incorrect current password" });

        var newHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        db.Execute("UPDATE Users SET PasswordHash = @newHash WHERE Id = @userId",
            new { newHash, userId });

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        using var db = _ctx.CreateConnection();

        var user = db.QuerySingleOrDefault(@"
            SELECT Id, Name, Email 
            FROM Users 
            WHERE LOWER(Email) = LOWER(@Email) AND IsActive = 1",
            new { req.Email });

        if (user != null)
        {
            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddMinutes(30);

            db.Execute(@"
                INSERT INTO PasswordResetTokens (Id, UserId, Token, ExpiresAt)
                VALUES (NEWID(), @UserId, @Token, @ExpiresAt)",
                new { UserId = user.Id, Token = token, ExpiresAt = expiresAt });

            // Try detecting the frontend URL from Origin/Referer (crucial for local dev where ports differ)
            var origin = Request.Headers.Origin.ToString();
            var referer = Request.Headers.Referer.ToString();
            
            var baseUrl = string.Empty;
            if (!string.IsNullOrEmpty(origin)) 
            {
                baseUrl = origin; 
            }
            else if (!string.IsNullOrEmpty(referer)) 
            {
                baseUrl = new Uri(referer).GetLeftPart(UriPartial.Authority);
            }
            else 
            {
                baseUrl = $"{Request.Scheme}://{Request.Host}";
            }
                
            // Frontend Link
            var resetLink = $"{baseUrl.TrimEnd('/')}/reset-password?token={token}";

            await _email.SendPasswordResetAsync(user.Email, resetLink);
        }

        // Always return success to prevent email enumeration
        return Ok(new { message = "If your email is registered, you will receive a reset link shortly." });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword(ResetPasswordRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        using var db = _ctx.CreateConnection();

        var reset = db.QuerySingleOrDefault<dynamic>(@"
            SELECT UserId, ExpiresAt 
            FROM PasswordResetTokens 
            WHERE Token = @Token AND IsUsed = 0",
            new { req.Token });

        if (reset == null || reset.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "Invalid or expired reset token" });

        var newHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        
        db.Execute(@"
            UPDATE Users SET PasswordHash = @newHash WHERE Id = @UserId;
            UPDATE PasswordResetTokens SET IsUsed = 1 WHERE Token = @Token;",
            new { newHash, UserId = reset.UserId, Token = req.Token });

        return Ok(new { message = "Password reset successfully" });
    }
}
