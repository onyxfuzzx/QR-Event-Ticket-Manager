using System;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace QREventPlatform.Advanced.Extensions;

public static class UserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var id =
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("UserId missing in token");   

        return Guid.Parse(id!);
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role)!;
    }
}
