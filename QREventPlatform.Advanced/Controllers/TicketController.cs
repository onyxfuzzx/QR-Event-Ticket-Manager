using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.DTOs;
using QREventPlatform.Advanced.Extensions;
using QREventPlatform.Advanced.Hubs;
using QREventPlatform.Advanced.Models;
using QREventPlatform.Advanced.Services;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketController : ControllerBase
{
    private readonly DapperContext _ctx;
    private readonly IHubContext<AdminHub> _hub;

    public TicketController(DapperContext ctx, IHubContext<AdminHub> hub)
    {
        _ctx = ctx;
        _hub = hub;
    }

    // ============================
    // ADMIN CREATES TICKET
    // ============================
    [Authorize(Roles = "Admin")]
    [HttpPost("{eventId}")]
    public async Task<IActionResult> Create(
        Guid eventId,
        [FromBody] CreateTicketRequest req,
        [FromServices] EmailService email
    )
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            return BadRequest("Customer email is required");

        using var db = _ctx.CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();

        var adminId = User.GetUserId();

        var eventInfo = db.QuerySingleOrDefault<EventInfoDto>(
            """
            SELECT Name
            FROM Events
            WHERE Id = @Id
              AND CreatedByAdminId = @AdminId
              AND IsActive = 1
            """,
            new { Id = eventId, AdminId = adminId },
            tx
        );

        if (eventInfo == null)
        {
            tx.Rollback();
            return NotFound("Event not found or not authorized");
        }

        var ticketId = Guid.NewGuid();
        var code = Guid.NewGuid().ToString("N");
        var qrUrl = $"https://qrevent-hyd4e9acbcfueufk.canadacentral-01.azurewebsites.net/qr/{code}";

        db.Execute(
            """
            INSERT INTO Tickets
            (Id, EventId, Code, IsActive)
            VALUES
            (@Id, @EventId, @Code, 1)
            """,
            new
            {
                Id = ticketId,
                EventId = eventId,
                Code = code
            },
            tx
        );

        // AUDIT
        db.Execute(
            """
            INSERT INTO AuditLogs
            (Id, UserId, Action, Entity, EntityId)
            VALUES
            (NEWID(), @UserId, 'CREATE', 'Ticket', @EntityId)
            """,
            new
            {
                UserId = adminId,
                EntityId = ticketId
            },
            tx
        );

        try
        {
            await email.SendTicketAsync(
                req.Email.Trim(),
                eventInfo.Name,
                code,
                qrUrl
            );

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }

        return Ok(new
        {
            ticketId,
            code,
            qrUrl
        });
    }

    // ============================
    // WORKER VALIDATES TICKET
    // ============================
    [Authorize(Roles = "Worker")]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateTicketRequest req)
    {
        using var db = _ctx.CreateConnection();
        db.Open();
        using var tx = db.BeginTransaction();

        var workerId = User.GetUserId();
        var workerName = db.ExecuteScalar<string>(@"
    SELECT Name
    FROM Users
    WHERE Id = @WorkerId
      AND IsActive = 1
", new { WorkerId = workerId }, tx);

        var ticketExists = db.ExecuteScalar<int>(
            @"
            SELECT COUNT(1)
            FROM Tickets
            WHERE Code = @Code
              AND IsActive = 1
            ",
            new { Code = req.Code },
            tx
        );

        if (ticketExists == 0)
        {
            tx.Rollback();
            return NotFound("Invalid ticket");
        }

        // ✅ STEP 1: STRICT ASSIGNMENT CHECK (FIRST!)

        var ticketCtx = db.QuerySingleOrDefault(@"
    SELECT
        t.Id        AS TicketId,
        t.Code      AS TicketCode,
        e.Id        AS EventId,
        e.Name      AS EventName,
        e.CreatedByAdminId AS AdminId
    FROM Tickets t
    JOIN Events e ON e.Id = t.EventId
    WHERE
        t.Code = @Code
        AND t.IsActive = 1
", new { Code = req.Code }, tx);

        if (ticketCtx == null)
        {
            tx.Rollback();
            return NotFound("Invalid ticket");
        }

        var assignment = db.QuerySingleOrDefault<TicketScanContext>(
            @"
            SELECT
                t.Id AS TicketId,
                t.Code AS TicketCode,
                e.Id AS EventId,
                e.Name AS EventName,
                e.CreatedByAdminId AS AdminId,
                w.Id AS WorkerId,
                w.Name AS WorkerName
            FROM Tickets t
            JOIN Events e
                ON e.Id = t.EventId
                AND e.IsActive = 1
            JOIN EventWorkers ew
                ON ew.EventId = t.EventId
                AND ew.WorkerId = @WorkerId
                AND ew.IsActive = 1
            JOIN Users w
                ON w.Id = ew.WorkerId
            WHERE
                t.Code = @Code
                AND t.IsActive = 1
            ",
            new
            {
                Code = req.Code,
                WorkerId = workerId
            },
            tx
        );

        if (assignment == null)
        {
            db.Execute(@"
        INSERT INTO TicketScanLogs (
            Id,
            TicketId,
            TicketCode,
            EventId,
            EventName,
            WorkerId,
            WorkerName,
            AdminId,
            ScanResult,
            ScanSource,
            ScannedAt
        )
        VALUES (
            NEWID(),
            @TicketId,
            @TicketCode,
            @EventId,
            @EventName,
            @WorkerId,
            @WorkerName,
            @AdminId,
            'UNAUTHORIZED',
            'MOBILE_QR',
            SYSUTCDATETIME()
        )
    ", new
            {
                ticketCtx.TicketId,
                ticketCtx.TicketCode,
                ticketCtx.EventId,
                ticketCtx.EventName,
                WorkerId = workerId,
                WorkerName = workerName,
                ticketCtx.AdminId
            }, tx);

            tx.Commit();
            return StatusCode(403, "Worker is not assigned to this event");
        }

        // ============================
        // STEP 2: TRY VALIDATE
        // ============================
        var validated = db.Execute(
            @"
            UPDATE t
            SET
                IsUsed = 1,
                UsedAt = SYSUTCDATETIME(),
                UsedByWorkerId = @WorkerId
            FROM Tickets t
            WHERE
                t.Id = @TicketId
                AND t.IsUsed = 0
            ",
            new
            {
                TicketId = assignment.TicketId,
                WorkerId = workerId
            },
            tx
        );

        // ============================
        // VALID
        // ============================
        if (validated == 1)
        {
            db.Execute(
                @"
                INSERT INTO TicketScanLogs (
                    Id,
                    TicketId,
                    TicketCode,
                    EventId,
                    EventName,
                    WorkerId,
                    WorkerName,
                    AdminId,
                    ScanResult,
                    ScanSource,
                    ScannedAt
                )
                VALUES (
                    NEWID(),
                    @TicketId,
                    @TicketCode,
                    @EventId,
                    @EventName,
                    @WorkerId,
                    @WorkerName,
                    @AdminId,
                    'VALID',
                    'MOBILE_QR',
                    SYSUTCDATETIME()
                )
                ",
                assignment,
                tx
            );

            tx.Commit();

            await _hub.Clients
                .Group($"ADMIN_{assignment.AdminId}")
                .SendAsync("TicketScanned", new
                {
                    ticketCode = assignment.TicketCode,
                    eventName = assignment.EventName,
                    workerName = assignment.WorkerName,
                    result = "VALID",
                    time = DateTime.UtcNow
                });

            return Ok(new { status = "VALID" });
        }
        // ============================
        // REVALIDATED
        // ============================
        db.Execute(
            @"
            INSERT INTO TicketRevalidations (
                Id,
                TicketId,
                WorkerId,
                RevalidatedAt
            )
            VALUES (
                NEWID(),
                @TicketId,
                @WorkerId,
                SYSUTCDATETIME()
            )
            ",
            new
            {
                TicketId = assignment.TicketId,
                WorkerId = workerId
            },
            tx
        );

        // ===================== REVALIDATED =====================
        db.Execute(
            @"
            INSERT INTO TicketScanLogs (
                Id,
                TicketId,
                TicketCode,
                EventId,
                EventName,
                WorkerId,
                WorkerName,
                AdminId,
                ScanResult,
                ScanSource,
                ScannedAt
            )
            VALUES (
                NEWID(),
                @TicketId,
                @TicketCode,
                @EventId,
                @EventName,
                @WorkerId,
                @WorkerName,
                @AdminId,
                'REVALIDATED',
                'MOBILE_QR',
                SYSUTCDATETIME()
            )
            ",
            assignment,
            tx
        );

        tx.Commit();

        await _hub.Clients
            .Group($"ADMIN_{assignment.AdminId}")
            .SendAsync("TicketScanned", new
            {
                ticketCode = assignment.TicketCode,
                eventName = assignment.EventName,
                workerName = assignment.WorkerName,
                result = "REVALIDATED",
                time = DateTime.UtcNow
            });

        return Ok(new { status = "REVALIDATED" });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("scan-history")]
    public IActionResult GetScanHistory()
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var logs = db.Query(
            """
            SELECT
                TicketCode,
                EventName,
                WorkerName,
                ScanResult,
                ScanSource,
                ScannedAt
            FROM TicketScanLogs
            WHERE AdminId = @AdminId
            ORDER BY ScannedAt DESC
            """,
            new { AdminId = adminId }
        );

        return Ok(logs);
    }

    // ============================
    // ADMIN DELETE TICKET
    // ============================
    [Authorize(Roles = "Admin")]
    [HttpDelete("{ticketId}")]
    public IActionResult Delete(Guid ticketId)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var rows = db.Execute(
            """
            UPDATE t
            SET IsActive = 0
            FROM Tickets t
            INNER JOIN Events e ON e.Id = t.EventId
            WHERE
                t.Id = @TicketId
                AND e.CreatedByAdminId = @AdminId
            """,
            new { TicketId = ticketId, AdminId = adminId }
        );

        if (rows == 0)
            return NotFound("Ticket not found or unauthorized");

        db.Execute(
            """
            INSERT INTO AuditLogs
            (Id, UserId, Action, Entity, EntityId)
            VALUES
            (NEWID(), @UserId, 'DELETE', 'Ticket', @EntityId)
            """,
            new
            {
                UserId = adminId,
                EntityId = ticketId
            }
        );

        return Ok("Ticket deleted");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{ticketId}/restore")]
    public IActionResult RestoreTicket(Guid ticketId)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var rows = db.Execute(
            """
            UPDATE t
            SET IsActive = 1
            FROM Tickets t
            INNER JOIN Events e ON e.Id = t.EventId
            WHERE t.Id = @TicketId
              AND e.CreatedByAdminId = @AdminId
            """,
            new { TicketId = ticketId, AdminId = adminId }
        );

        if (rows == 0)
            return NotFound("Ticket not found or unauthorized");

        return Ok(new { success = true });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("deleted")]
    public IActionResult GetDeletedTickets()
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var tickets = db.Query(
            """
            SELECT
                t.Id,
                t.Code,
                t.CreatedAt,
                e.Name AS EventName
            FROM Tickets t
            INNER JOIN Events e ON e.Id = t.EventId
            WHERE t.IsActive = 0
              AND e.CreatedByAdminId = @AdminId
            ORDER BY t.CreatedAt DESC
            """,
            new { AdminId = adminId }
        );

        return Ok(tickets);
    }

    // ============================
    // ADMIN – GET EVENT TICKETS + STATS
    // ============================
    [Authorize(Roles = "Admin")]
    [HttpGet("event/{eventId}")]
    public IActionResult GetEventTickets(Guid eventId)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        // 🔐 Verify admin owns the event
        var exists = db.ExecuteScalar<int>(
            @"
            SELECT COUNT(1)
            FROM Events
            WHERE Id = @EventId
              AND CreatedByAdminId = @AdminId
              AND IsActive = 1
            ",
            new { EventId = eventId, AdminId = adminId }
        );

        if (exists == 0)
            return NotFound("Event not found or unauthorized");

        // 📊 Ticket stats (FOR THIS EVENT ONLY)
        var stats = db.QuerySingle<EventTicketStats>(
            @"
            SELECT
                COUNT(*) AS TotalTickets,
                SUM(CASE WHEN IsUsed = 1 THEN 1 ELSE 0 END) AS UsedTickets,
                (
                    SELECT COUNT(*)
                    FROM TicketRevalidations tr
                    JOIN Tickets t2 ON t2.Id = tr.TicketId
                    WHERE t2.EventId = @EventId
                ) AS Revalidations
            FROM Tickets
            WHERE EventId = @EventId
              AND IsActive = 1
            ",
            new { EventId = eventId }
        );

        // 🎫 Tickets list
        var tickets = db.Query<EventTicketDto>(
            @"
            SELECT
                Id,
                Code,
                IsUsed,
                CreatedAt,
                CONCAT('https://
/qr/', Code) AS QrUrl
            FROM Tickets
            WHERE EventId = @EventId
              AND IsActive = 1
            ORDER BY CreatedAt DESC
            ",
            new { EventId = eventId }
        );

        return Ok(new EventTicketsResponse
        {
            Stats = stats,
            Tickets = tickets
        });
    }
}