using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/superadmin/dashboard")]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminDashboardController : ControllerBase
{
    private readonly DapperContext _ctx;

    public SuperAdminDashboardController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    // ============================
    // SYSTEM SUMMARY
    // ============================
    [HttpGet("summary")]
    public IActionResult Summary()
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        var adminRole = (int)Enums.Role.Admin;

        var data = db.QuerySingle(
    """
    SELECT
        (SELECT COUNT(*) 
         FROM Users 
         WHERE Role = @AdminRole AND IsActive = 1) AS TotalAdmins,

        (SELECT COUNT(*) 
         FROM Events 
         WHERE IsActive = 1) AS TotalEvents,

        (SELECT COUNT(*) 
         FROM Tickets 
         WHERE IsActive = 1) AS TotalTickets,

        (SELECT COUNT(*) 
         FROM Tickets 
         WHERE IsActive = 1 AND IsUsed = 1) AS UsedTickets,

        (SELECT COUNT(*)
        FROM TicketRevalidations tr
        JOIN Tickets t ON t.Id = tr.TicketId
        WHERE t.IsActive = 1) AS Revalidations

    """,
    new { AdminRole = adminRole }
        );

        return Ok(data);
    }

    // ============================
    // ADMINS LIST
    // ============================
    [HttpGet("admins")]
    public IActionResult GetAdmins()
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        var adminRole = (int)Enums.Role.Admin;

        var admins = db.Query(
            """
            SELECT
                Id,
                Name,
                Email,
                IsActive,
                CreatedAt
            FROM Users
            WHERE Role = @AdminRole 
            AND IsActive = 1        
            ORDER BY CreatedAt DESC
            """,
            new { AdminRole = adminRole }
        );

        return Ok(admins);
    }

    // ============================
    // ADMIN ACTIVITY
    // ============================
    [HttpGet("admins/activity")]
    public IActionResult AdminActivity()
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        var adminRole = (int)Enums.Role.Admin;

        var activity = db.Query(
            """
            SELECT
                u.Name AS AdminName,
                COUNT(DISTINCT e.Id) AS EventsCreated,
                COUNT(DISTINCT t.Id) AS TicketsCreated
            FROM Users u
            LEFT JOIN Events e 
                   ON e.CreatedByAdminId = u.Id AND e.IsActive = 1
            LEFT JOIN Tickets t 
                   ON t.EventId = e.Id AND t.IsActive = 1
            WHERE u.Role = @AdminRole
            AND u.IsActive = 1
            GROUP BY u.Name
            ORDER BY EventsCreated DESC
            """,
            new { AdminRole = adminRole }
        );

        return Ok(activity);
    }
}