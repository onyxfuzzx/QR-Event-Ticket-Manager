using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Extensions;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly DapperContext _ctx;

    public AdminDashboardController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    // ============================
    // ADMIN SUMMARY
    // ============================
    [HttpGet("summary")]
    public IActionResult Summary()
    {
        using var db = _ctx.CreateConnection();
        db.Open();
        var adminId = User.GetUserId();

        var data = db.QuerySingle("""
            SELECT
                (SELECT COUNT(*) FROM Events WHERE CreatedByAdminId = @AdminId AND IsActive = 1) AS events,
                (SELECT COUNT(*) FROM Tickets t INNER JOIN Events e ON e.Id = t.EventId WHERE e.CreatedByAdminId = @AdminId AND e.IsActive = 1 AND t.IsActive = 1) AS tickets,
                (SELECT COUNT(*) FROM Tickets t INNER JOIN Events e ON e.Id = t.EventId WHERE e.CreatedByAdminId = @AdminId AND e.IsActive = 1 AND t.IsActive = 1 AND t.IsUsed = 1) AS usedTickets,
                (SELECT COUNT(*) FROM TicketRevalidations r INNER JOIN Tickets t ON t.Id = r.TicketId INNER JOIN Events e ON e.Id = t.EventId WHERE e.CreatedByAdminId = @AdminId AND e.IsActive = 1 AND t.IsActive = 1) AS revalidations,
                (SELECT COUNT(*) FROM Users WHERE Role = 2 AND CreatedByAdminId = @AdminId AND IsActive = 1) AS workers
        """, new { AdminId = adminId });

        return Ok(data);
    }

    // ============================
    // EVENT STATS
    // ============================
    [HttpGet("events")]
    public IActionResult Events()
    {
        using var db = _ctx.CreateConnection();
        db.Open();
        var adminId = User.GetUserId();

        var events = db.Query("""
            SELECT
                e.Id as id,
                e.Name as name,
                e.EventDate as eventDate,
                e.Location as location,
                COUNT(DISTINCT t.Id) AS tickets,
                COALESCE(SUM(CASE WHEN t.IsUsed = 1 THEN 1 ELSE 0 END), 0) AS usedTickets
            FROM Events e
            LEFT JOIN Tickets t 
                   ON t.EventId = e.Id AND t.IsActive = 1
            WHERE e.CreatedByAdminId = @AdminId
              AND e.IsActive = 1
            GROUP BY 
                e.Id,
                e.Name,
                e.EventDate,
                e.Location
            ORDER BY e.EventDate DESC
        """, new { AdminId = adminId });

        return Ok(events);
    }

    // ============================
    // WORKER ACTIVITY
    // ============================
    [HttpGet("workers")]
    public IActionResult Workers()
    {
        using var db = _ctx.CreateConnection();
        db.Open();
        var adminId = User.GetUserId();
        var workerRole = (int)Enums.Role.Worker;

        var workers = db.Query("""
            SELECT
                u.Id,
                u.Name,
                COUNT(DISTINCT t.Id) AS Scans,
                COUNT(DISTINCT r.Id) AS Revalidations
            FROM Users u
            INNER JOIN EventWorkers ew 
                    ON ew.WorkerId = u.Id AND ew.IsActive = 1
            INNER JOIN Events e 
                    ON e.Id = ew.EventId AND e.IsActive = 1
            LEFT JOIN Tickets t 
                    ON t.UsedByWorkerId = u.Id AND t.IsActive = 1
            LEFT JOIN TicketRevalidations r 
                    ON r.WorkerId = u.Id
            WHERE u.Role = @WorkerRole
              AND u.IsActive = 1
              AND e.CreatedByAdminId = @AdminId
            GROUP BY u.Id, u.Name
        """, new
        {
            AdminId = adminId,
            WorkerRole = workerRole
        });

        return Ok(workers);
    }

    // ============================
    // REVALIDATION ALERTS
    // ============================
    [HttpGet("revalidations")]
    public IActionResult Revalidations()
    {
        using var db = _ctx.CreateConnection();
        db.Open();
        var adminId = User.GetUserId();

        var alerts = db.Query("""
            SELECT
                t.Code,
                u.Name AS WorkerName,
                r.RevalidatedAt
            FROM TicketRevalidations r
            INNER JOIN Tickets t 
                    ON t.Id = r.TicketId AND t.IsActive = 1
            INNER JOIN Users u 
                    ON u.Id = r.WorkerId AND u.IsActive = 1
            INNER JOIN Events e 
                    ON e.Id = t.EventId AND e.IsActive = 1
            WHERE e.CreatedByAdminId = @AdminId
            ORDER BY r.RevalidatedAt DESC
        """, new { AdminId = adminId });

        return Ok(alerts);
    }
}
