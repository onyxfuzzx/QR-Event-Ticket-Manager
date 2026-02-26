using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Extensions;
using System.Collections.Generic;
using System.Threading;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase
{
    private readonly DapperContext _ctx;

    public NotificationsController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    // ============================
    // GET MY NOTIFICATIONS
    // ============================
    [HttpGet]
    public IActionResult GetMyNotifications()
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        var adminId = User.GetUserId();

        var notifications = db.Query("""
        SELECT
            Id,
            Message,
            IsRead,
            CreatedAt
        FROM Notifications
        WHERE UserId = @UserId
        ORDER BY CreatedAt DESC
    """, new { UserId = adminId });

        return Ok(notifications);
    }


    [HttpGet("unread-count")]
    public IActionResult GetUnreadCount()
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        var adminId = User.GetUserId();

        var count = db.ExecuteScalar<int>("""
        SELECT COUNT(*)
        FROM Notifications
        WHERE UserId = @UserId
          AND IsRead = 0
    """, new { UserId = adminId });

        return Ok(new { unread = count });
    }


    // ============================
    // MARK ONE AS READ
    // ============================
    [HttpPost("read/{id}")]
    public IActionResult MarkAsRead(Guid id)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var rows = db.Execute(
            """
    
            UPDATE Notifications SET IsRead = 1 WHERE Id = @Id AND UserId = @UserId
    
            """, new
          { Id = id, UserId = adminId });

        if (rows == 0)
            return NotFound("Notification not found");

        return Ok("Marked as read");
    }

    // ============================
    // MARK ALL AS READ
    // ============================
    [HttpPost("read-all")]
    public IActionResult MarkAllAsRead()
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        db.Execute("""
            UPDATE Notifications
            SET IsRead = 1
            WHERE UserId = @UserId
        """, new { UserId = adminId });

        return Ok("All notifications marked as read");
    }
}
