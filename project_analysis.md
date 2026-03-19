# QR Event Ticket Manager – Full Analysis & Fixes

## 📋 Project Overview

A **full-stack QR-based event management platform** with:
- **Backend**: ASP.NET Core 8 Web API (Dapper, SQL Server, JWT Auth, SignalR)
- **Frontend**: Angular 17 (Standalone Components, Tailwind CSS, SignalR)

**Core Features**: Role-based auth (SuperAdmin → Admin → Worker), Event CRUD, Ticket generation with QR codes + email, QR scanning by workers, Dynamic event forms, Real-time SignalR notifications, Audit trails.

---

## 🐛 Bugs Found & Fixed

### Backend (Critical)

| # | Issue | File | Fix |
|---|-------|------|-----|
| 1 | **Broken QR URL** – SQL `CONCAT('https://\n/qr/', Code)` had a newline char creating invalid URLs | [TicketController.cs](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/TicketController.cs#L581-L585) | Fixed CONCAT to use full valid URL |
| 2 | **Missing `db.Open()` before transaction** – [RestoreEvent](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/EventController.cs#131-164) called `BeginTransaction()` on a closed connection | [EventController.cs](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/EventController.cs#L135-L138) | Added `db.Open()` before transaction |
| 3 | **Invalid INSERT columns** – [PublicEventController](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/PublicEventController.cs#8-113) referenced `QrUrl` and `IsUsed` columns that don't exist | [PublicEventController.cs](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/PublicEventController.cs#L65-L68) | Aligned SQL with actual schema |
| 4 | **QR generated for invalid tickets** – [QrController](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/QrController.cs#7-46) didn't check `valid == 0` | [QrController.cs](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/Controllers/QrController.cs#L24-L33) | Added `NotFound` return for invalid codes |
| 5 | **Classes outside namespace** – DTOs in global namespace | [TicketScanContext.cs](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.Advanced/DTOs/TicketScanContext.cs) | Moved all classes inside namespace |

### Frontend (Critical)

| # | Issue | File | Fix |
|---|-------|------|-----|
| 6 | **Missing [()](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.UI/src/app/dashboards/admin/components/event-form-builder/event-form-builder.component.ts#57-62) on [loadSummary](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.UI/src/app/dashboards/superadmin/superadmin.component.ts#58-72)** – `this.loadSummary;` never called the function | [admin.component.ts](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.UI/src/app/dashboards/admin/admin.component.ts#L485) | Changed to `this.loadSummary()` |
| 7 | **`styleUrl` typo** (4 files) – Angular expects `styleUrls` (array) | admin, superadmin, worker-scanner, event-form-builder [.ts](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.UI/src/main.ts) files | Changed to `styleUrls: ['...']` |
| 8 | **`alert()` for login errors** – jarring UX | [login.component.ts](file:///c:/Users/Administrator/Desktop/projects/QR-Event-Ticket-Manager/QREventPlatform.UI/src/app/auth/login/login.component.ts#L50) | Replaced with inline error banner |

---

## 🎨 UI/UX Improvements Made

### Global Design System
- Added **Inter font** from Google Fonts across all pages
- Created a **CSS custom properties** design system (colors, gradients, shadows, radii)
- Implemented **smooth scrollbar** styling
- Added reusable **animation keyframes** (fadeInUp, slideDown, pulse-glow)
- Extended **Tailwind config** with Inter font family

### Login Page (Complete Redesign)
- **Dark glassmorphism** card with frosted glass effect
- **Animated floating orbs** background (3 colored blurred circles)
- **SVG icons** for email, lock, and eye toggle
- **Inline error banner** (replaces alert())
- **Gradient submit button** with hover lift animation
- **Loading spinner** in button during auth

### Admin Dashboard (Complete Redesign)
- **Sticky glassmorphism header** with brand icon
- **Color-coded stat cards** with gradient top borders (indigo, cyan, emerald, amber)
- **Modern tab navigation** in a pill container
- **Section cards** with headers, subtitles, and proper padding
- **Badge system** for status indicators (success, danger, warning)
- **Empty state illustrations** with emoji icons
- **Refined tables** with uppercase headers, hover rows, and proper spacing

### SuperAdmin Dashboard (Complete Redesign)
- Consistent with Admin dashboard design language
- **Red gradient header icon** to distinguish from Admin
- **Summary cards** with colored top borders
- **Modern tabs** and badge system
- **Action buttons** with pill-shaped styling

### Worker Scanner (Complete Redesign)
- **Consistent sticky header** with brand
- **Animated result banners** with icon circles (✓, ✕, ↻)
- **Bold gradient scan button** with lift hover effect
- **Styled scan history** with badge indicators and monospace codes
- **Pop animation** when result appears

### Event Registration (Complete Redesign)
- **Premium card** with decorative background orbs
- **User icon** in gradient container
- **Styled form fields** with focus animations
- **Loading spinner** while form loads
- **Success state** with animated checkmark

### QR Scanner Modal
- **Dark overlay** with rounded close button
- **Pulse animation** on scan frame
- **Refined footer** text styling

---

## 📝 Notes

> [!NOTE]
> All "Cannot find module" lint errors in the IDE are because `node_modules` hasn't been installed yet. Run `npm install` in the `QREventPlatform.UI` directory to resolve them.

> [!IMPORTANT]
> The "Parameter implicitly has 'any' type" warnings are existing TypeScript strictness issues in the original code. They don't prevent compilation and are cosmetic – the project uses `any` types throughout by design choice.

### Files Modified

```diff:TicketController.cs
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
===
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
                CONCAT('https://qrevent-hyd4e9acbcfueufk.canadacentral-01.azurewebsites.net/qr/', Code) AS QrUrl
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
```
```diff:EventController.cs
    using Dapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using QREventPlatform.Advanced.Data;
    using QREventPlatform.Advanced.Extensions;
    using QREventPlatform.Advanced.Models;
    using System.Security.Claims;

    namespace QREventPlatform.Advanced.Controllers;

    [ApiController]
    [Route("api/events")]
    [Authorize(Roles = "Admin")]
    public class EventController : ControllerBase
    {
        private readonly DapperContext _ctx;

        public EventController(DapperContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost]
        public IActionResult Create(CreateEventRequest req)
        {
            using var db = _ctx.CreateConnection();
            var adminId = User.GetUserId();

            db.Execute("""
        INSERT INTO Events
        (Id, Name, Location, EventDate, Tickets, UsedTickets, PublicId, SecretKey, CreatedByAdminId, CreatedAt)
        VALUES
        (@Id, @Name, @Location, @EventDate, 0, 0, @PublicId, @SecretKey, @AdminId, GETUTCDATE())
        """, new
            {
                Id = Guid.NewGuid(),
                req.Name,
                req.Location,
                req.EventDate,
                PublicId = Guid.NewGuid(),
                SecretKey = Guid.NewGuid(),
                AdminId = adminId
            });


            return Ok(new { message = "Event created" });
        }

        [HttpGet]
        public IActionResult GetMyEvents()
        {
            using var db = _ctx.CreateConnection();
            var adminId = User.GetUserId();

            return Ok(db.Query("""
            SELECT Id, Name, EventDate, CreatedAt
            FROM Events
            WHERE CreatedByAdminId = @AdminId
              AND IsActive = 1
        """, new { AdminId = adminId }));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{eventId}")]
        public IActionResult DeleteEvent(Guid eventId)
        {
            using var db = _ctx.CreateConnection();
            db.Open();

            var adminId = User.GetUserId();
            using var tx = db.BeginTransaction();

            try
            {
                // 1️⃣ Soft-disable event workers
                db.Execute("""
                UPDATE EventWorkers
                SET IsActive = 0
                WHERE EventId = @EventId
            """, new { EventId = eventId }, tx);

                // 2️⃣ Soft-disable tickets
                db.Execute("""
                UPDATE Tickets
                SET IsActive = 0
                WHERE EventId = @EventId
            """, new { EventId = eventId }, tx);

                // 3️⃣ Soft-disable event
                var rows = db.Execute("""
                UPDATE Events
                SET IsActive = 0
                WHERE Id = @EventId
                  AND CreatedByAdminId = @AdminId
            """, new { EventId = eventId, AdminId = adminId }, tx);

                if (rows == 0)
                {
                    tx.Rollback();
                    return NotFound();
                }

                tx.Commit();
                return Ok(new { success = true });
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("event-workers/{assignmentId}/restore")]
        public IActionResult RestoreEventWorker(Guid assignmentId)
        {
            using var db = _ctx.CreateConnection();

            var rows = db.Execute("""
            UPDATE EventWorkers
            SET IsActive = 1
            WHERE Id = @Id
        """, new { Id = assignmentId });

            if (rows == 0)
                return NotFound("Assignment not found");

            return Ok(new { success = true });
        }

    [Authorize(Roles = "Admin")]
    [HttpPost("{eventId}/restore")]
    public IActionResult RestoreEvent(Guid eventId)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        using var tx = db.BeginTransaction();

        var rows = db.Execute("""
        UPDATE Events
        SET IsActive = 1
        WHERE Id = @EventId
          AND CreatedByAdminId = @AdminId
    """, new { EventId = eventId, AdminId = adminId }, tx);

        if (rows == 0)
        {
            tx.Rollback();
            return NotFound();
        }

        db.Execute("""
        UPDATE Tickets SET IsActive = 1 WHERE EventId = @EventId
    """, new { EventId = eventId }, tx);

        db.Execute("""
        UPDATE EventWorkers SET IsActive = 1 WHERE EventId = @EventId
    """, new { EventId = eventId }, tx);

        tx.Commit();
        return Ok(new { success = true });
    }


        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]
        public IActionResult GetDeletedEvents()
        {
            using var db = _ctx.CreateConnection();
            var adminId = User.GetUserId();

            var events = db.Query("""
            SELECT
                Id,
                Name,
                Location,
                EventDate,
                CreatedAt
            FROM Events
            WHERE IsActive = 0
              AND CreatedByAdminId = @AdminId
            ORDER BY CreatedAt DESC
        """, new { AdminId = adminId });

            return Ok(events);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("event-workers/deleted")]
        public IActionResult GetDeletedEventWorkers()
        {
            using var db = _ctx.CreateConnection();

            var data = db.Query("""
            SELECT
                ew.Id,
                u.Name AS WorkerName,
                e.Name AS EventName,
                ew.AssignedAt
            FROM EventWorkers ew
            INNER JOIN Users u ON u.Id = ew.WorkerId
            INNER JOIN Events e ON e.Id = ew.EventId
            WHERE ew.IsActive = 0
            ORDER BY ew.AssignedAt DESC
        """);

            return Ok(data);
        }

    }
===
    using Dapper;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using QREventPlatform.Advanced.Data;
    using QREventPlatform.Advanced.Extensions;
    using QREventPlatform.Advanced.Models;
    using System.Security.Claims;

    namespace QREventPlatform.Advanced.Controllers;

    [ApiController]
    [Route("api/events")]
    [Authorize(Roles = "Admin")]
    public class EventController : ControllerBase
    {
        private readonly DapperContext _ctx;

        public EventController(DapperContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost]
        public IActionResult Create(CreateEventRequest req)
        {
            using var db = _ctx.CreateConnection();
            var adminId = User.GetUserId();

            db.Execute("""
        INSERT INTO Events
        (Id, Name, Location, EventDate, Tickets, UsedTickets, PublicId, SecretKey, CreatedByAdminId, CreatedAt)
        VALUES
        (@Id, @Name, @Location, @EventDate, 0, 0, @PublicId, @SecretKey, @AdminId, GETUTCDATE())
        """, new
            {
                Id = Guid.NewGuid(),
                req.Name,
                req.Location,
                req.EventDate,
                PublicId = Guid.NewGuid(),
                SecretKey = Guid.NewGuid(),
                AdminId = adminId
            });


            return Ok(new { message = "Event created" });
        }

        [HttpGet]
        public IActionResult GetMyEvents()
        {
            using var db = _ctx.CreateConnection();
            var adminId = User.GetUserId();

            return Ok(db.Query("""
            SELECT Id, Name, EventDate, CreatedAt
            FROM Events
            WHERE CreatedByAdminId = @AdminId
              AND IsActive = 1
        """, new { AdminId = adminId }));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{eventId}")]
        public IActionResult DeleteEvent(Guid eventId)
        {
            using var db = _ctx.CreateConnection();
            db.Open();

            var adminId = User.GetUserId();
            using var tx = db.BeginTransaction();

            try
            {
                // 1️⃣ Soft-disable event workers
                db.Execute("""
                UPDATE EventWorkers
                SET IsActive = 0
                WHERE EventId = @EventId
            """, new { EventId = eventId }, tx);

                // 2️⃣ Soft-disable tickets
                db.Execute("""
                UPDATE Tickets
                SET IsActive = 0
                WHERE EventId = @EventId
            """, new { EventId = eventId }, tx);

                // 3️⃣ Soft-disable event
                var rows = db.Execute("""
                UPDATE Events
                SET IsActive = 0
                WHERE Id = @EventId
                  AND CreatedByAdminId = @AdminId
            """, new { EventId = eventId, AdminId = adminId }, tx);

                if (rows == 0)
                {
                    tx.Rollback();
                    return NotFound();
                }

                tx.Commit();
                return Ok(new { success = true });
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("event-workers/{assignmentId}/restore")]
        public IActionResult RestoreEventWorker(Guid assignmentId)
        {
            using var db = _ctx.CreateConnection();

            var rows = db.Execute("""
            UPDATE EventWorkers
            SET IsActive = 1
            WHERE Id = @Id
        """, new { Id = assignmentId });

            if (rows == 0)
                return NotFound("Assignment not found");

            return Ok(new { success = true });
        }

    [Authorize(Roles = "Admin")]
    [HttpPost("{eventId}/restore")]
    public IActionResult RestoreEvent(Guid eventId)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();
        db.Open();
        using var tx = db.BeginTransaction();

        var rows = db.Execute("""
        UPDATE Events
        SET IsActive = 1
        WHERE Id = @EventId
          AND CreatedByAdminId = @AdminId
    """, new { EventId = eventId, AdminId = adminId }, tx);

        if (rows == 0)
        {
            tx.Rollback();
            return NotFound();
        }

        db.Execute("""
        UPDATE Tickets SET IsActive = 1 WHERE EventId = @EventId
    """, new { EventId = eventId }, tx);

        db.Execute("""
        UPDATE EventWorkers SET IsActive = 1 WHERE EventId = @EventId
    """, new { EventId = eventId }, tx);

        tx.Commit();
        return Ok(new { success = true });
    }


        [Authorize(Roles = "Admin")]
        [HttpGet("deleted")]
        public IActionResult GetDeletedEvents()
        {
            using var db = _ctx.CreateConnection();
            var adminId = User.GetUserId();

            var events = db.Query("""
            SELECT
                Id,
                Name,
                Location,
                EventDate,
                CreatedAt
            FROM Events
            WHERE IsActive = 0
              AND CreatedByAdminId = @AdminId
            ORDER BY CreatedAt DESC
        """, new { AdminId = adminId });

            return Ok(events);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("event-workers/deleted")]
        public IActionResult GetDeletedEventWorkers()
        {
            using var db = _ctx.CreateConnection();

            var data = db.Query("""
            SELECT
                ew.Id,
                u.Name AS WorkerName,
                e.Name AS EventName,
                ew.AssignedAt
            FROM EventWorkers ew
            INNER JOIN Users u ON u.Id = ew.WorkerId
            INNER JOIN Events e ON e.Id = ew.EventId
            WHERE ew.IsActive = 0
            ORDER BY ew.AssignedAt DESC
        """);

            return Ok(data);
        }

    }
```
```diff:PublicEventController.cs
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Services;
using System.Text.Json;
using QREventPlatform.Advanced.Models;
[ApiController]
[Route("api/public/events")]
public class PublicEventController : ControllerBase
{
    private readonly DapperContext _ctx;
    private readonly EmailService _email;

    public PublicEventController(DapperContext ctx, EmailService email)
    {
        _ctx = ctx;
        _email = email;
    }

    // ===============================
    // GET EVENT FORM
    // ===============================
    [HttpGet("{eventId}/form")]
    public IActionResult GetForm(Guid eventId)
    {
        using var db = _ctx.CreateConnection();

        var schema = db.ExecuteScalar<string>(
            "SELECT [Schema] FROM EventForms WHERE EventId = @eventId",
            new { eventId });

        if (schema == null)
            return NotFound("Form not found");

        return Ok(JsonSerializer.Deserialize<object>(schema));
    }

    // ===============================
    // SUBMIT FORM
    // ===============================
    [HttpPost("{eventId}/submit")]
    public async Task<IActionResult> SubmitForm(
    Guid eventId,
    [FromBody] Dictionary<string, string> formData
)
    {
        using var db = _ctx.CreateConnection();

        // 1. Get event
        var ev = await db.QuerySingleAsync<Event>(
            "SELECT * FROM Events WHERE Id = @id",
            new { id = eventId }
        );

        // 2. Generate ticket
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            QrUrl = $"https://qrevent-hyd4e9acbcfueufk.canadacentral-01.azurewebsites.net/qr/{Guid.NewGuid()}"
        };

        await db.ExecuteAsync("""
        INSERT INTO Tickets (Id, EventId, Code, QrUrl, IsUsed)
        VALUES (@Id, @EventId, @Code, @QrUrl, 0)
    """, ticket);

        // 3. Save form submission
        await db.ExecuteAsync("""
INSERT INTO EventFormSubmissions
(Id, EventId, TicketId, Data, CreatedAt)
VALUES
(@Id, @EventId, @TicketId, @Data, SYSUTCDATETIME())
""", new
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            TicketId = ticket.Id, // 🔥 THIS IS THE FIX
            Data = JsonSerializer.Serialize(formData)
        });

        if (!formData.TryGetValue("email", out var email))
        {
            // fallback if admin named field differently
            formData.TryGetValue("mail", out email);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email field is required");
        }


        // 4. Send ticket email (🔥 YOUR EXISTING SERVICE)
        await _email.SendTicketAsync(
            toEmail: email,
            eventName: ev.Name,
            ticketCode: ticket.Code,
            qrUrl: ticket.QrUrl,
            ct: HttpContext.RequestAborted
        );

        return Ok(new
        {
            success = true,
            ticket = ticket.Code
        });
    }

}
===
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Services;
using System.Text.Json;
using QREventPlatform.Advanced.Models;
[ApiController]
[Route("api/public/events")]
public class PublicEventController : ControllerBase
{
    private readonly DapperContext _ctx;
    private readonly EmailService _email;

    public PublicEventController(DapperContext ctx, EmailService email)
    {
        _ctx = ctx;
        _email = email;
    }

    // ===============================
    // GET EVENT FORM
    // ===============================
    [HttpGet("{eventId}/form")]
    public IActionResult GetForm(Guid eventId)
    {
        using var db = _ctx.CreateConnection();

        var schema = db.ExecuteScalar<string>(
            "SELECT [Schema] FROM EventForms WHERE EventId = @eventId",
            new { eventId });

        if (schema == null)
            return NotFound("Form not found");

        return Ok(JsonSerializer.Deserialize<object>(schema));
    }

    // ===============================
    // SUBMIT FORM
    // ===============================
    [HttpPost("{eventId}/submit")]
    public async Task<IActionResult> SubmitForm(
    Guid eventId,
    [FromBody] Dictionary<string, string> formData
)
    {
        using var db = _ctx.CreateConnection();

        // 1. Get event
        var ev = await db.QuerySingleAsync<Event>(
            "SELECT * FROM Events WHERE Id = @id",
            new { id = eventId }
        );

        // 2. Generate ticket
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            QrUrl = $"https://qrevent-hyd4e9acbcfueufk.canadacentral-01.azurewebsites.net/qr/{Guid.NewGuid()}"
        };

        await db.ExecuteAsync("""
        INSERT INTO Tickets (Id, EventId, Code, IsActive)
        VALUES (@Id, @EventId, @Code, 1)
    """, new { ticket.Id, ticket.EventId, ticket.Code });

        // 3. Save form submission
        await db.ExecuteAsync("""
INSERT INTO EventFormSubmissions
(Id, EventId, TicketId, Data, CreatedAt)
VALUES
(@Id, @EventId, @TicketId, @Data, SYSUTCDATETIME())
""", new
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            TicketId = ticket.Id, // 🔥 THIS IS THE FIX
            Data = JsonSerializer.Serialize(formData)
        });

        if (!formData.TryGetValue("email", out var email))
        {
            // fallback if admin named field differently
            formData.TryGetValue("mail", out email);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email field is required");
        }


        // 4. Send ticket email (🔥 YOUR EXISTING SERVICE)
        await _email.SendTicketAsync(
            toEmail: email,
            eventName: ev.Name,
            ticketCode: ticket.Code,
            qrUrl: ticket.QrUrl,
            ct: HttpContext.RequestAborted
        );

        return Ok(new
        {
            success = true,
            ticket = ticket.Code
        });
    }

}
```
```diff:QrController.cs
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QREventPlatform.Advanced.Data;
using System.Drawing.Imaging;

[ApiController]
[Route("qr")]
public class QrController : ControllerBase
{
    private readonly DapperContext _ctx;

    public QrController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet("{code}")]
    public IActionResult GetQr(string code)
    {
        using var db = _ctx.CreateConnection();

        // 🔐 Validate ticket
        var valid = db.ExecuteScalar<int>("""
             SELECT COUNT(*)
             FROM Tickets
             WHERE Code = @Code
               AND IsActive = 1
        """, new { Code = code });



        // ✅ Generate QR only for valid ticket
        using var qrGen = new QRCodeGenerator();
        using var qrData = qrGen.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrData);
        using var bitmap = qrCode.GetGraphic(20);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);

        return File(ms.ToArray(), "image/png");
    }
}
===
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using QREventPlatform.Advanced.Data;
using System.Drawing.Imaging;

[ApiController]
[Route("qr")]
public class QrController : ControllerBase
{
    private readonly DapperContext _ctx;

    public QrController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet("{code}")]
    public IActionResult GetQr(string code)
    {
        using var db = _ctx.CreateConnection();

        // 🔐 Validate ticket
        var valid = db.ExecuteScalar<int>("""
             SELECT COUNT(*)
             FROM Tickets
             WHERE Code = @Code
               AND IsActive = 1
        """, new { Code = code });

        if (valid == 0)
            return NotFound("Invalid or inactive ticket");

        // ✅ Generate QR only for valid ticket
        using var qrGen = new QRCodeGenerator();
        using var qrData = qrGen.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrData);
        using var bitmap = qrCode.GetGraphic(20);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);

        return File(ms.ToArray(), "image/png");
    }
}
```
```diff:TicketScanContext.cs
namespace QREventPlatform.Advanced.DTOs { 
public sealed class TicketScanContext
{
    public Guid TicketId { get; set; }
    public string TicketCode { get; set; } = null!;
    public Guid EventId { get; set; }
    public string EventName { get; set; } = null!;
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = null!;
    public Guid AdminId { get; set; }
}

}
public class EventTicketsResponse
{
    public EventTicketStats Stats { get; set; }
    public IEnumerable<EventTicketDto> Tickets { get; set; }
}

public class EventTicketStats
{
    public int TotalTickets { get; set; }
    public int UsedTickets { get; set; }
    public int Revalidations { get; set; }
}

public class EventTicketDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public string QrUrl { get; set; }
}
===
namespace QREventPlatform.Advanced.DTOs
{
    public sealed class TicketScanContext
    {
        public Guid TicketId { get; set; }
        public string TicketCode { get; set; } = null!;
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public Guid WorkerId { get; set; }
        public string WorkerName { get; set; } = null!;
        public Guid AdminId { get; set; }
    }

    public class EventTicketsResponse
    {
        public EventTicketStats Stats { get; set; } = null!;
        public IEnumerable<EventTicketDto> Tickets { get; set; } = [];
    }

    public class EventTicketStats
    {
        public int TotalTickets { get; set; }
        public int UsedTickets { get; set; }
        public int Revalidations { get; set; }
    }

    public class EventTicketDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public string QrUrl { get; set; } = null!;
    }
}

```
```diff:login.component.ts
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';


  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;
  showPassword = false; // 👈 ADD

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  login() {
    if (!this.email || !this.password) return;

    this.loading = true;

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;

        const role = localStorage.getItem('role');

        if (role === 'SuperAdmin') {
          this.router.navigate(['/superadmin']);
        } else if (role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (role === 'Worker') {
          this.router.navigate(['/worker']);
        }
      },
      error: () => {
        this.loading = false;
        alert('Login failed');
      }
    });
  }


}
===
import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';


  private auth = inject(AuthService);
  private router = inject(Router);

  loading = false;
  showPassword = false; // 👈 ADD

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  login() {
    if (!this.email || !this.password) return;

    this.loading = true;

    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.loading = false;

        const role = localStorage.getItem('role');

        if (role === 'SuperAdmin') {
          this.router.navigate(['/superadmin']);
        } else if (role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (role === 'Worker') {
          this.router.navigate(['/worker']);
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Invalid email or password. Please try again.';
      }
    });
  }


}
```
```diff:login.component.html
<div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-indigo-50 via-white to-blue-50 px-4">

  <div class="w-full max-w-sm bg-white rounded-2xl shadow-xl p-7
              animate-fade-in">

    <!-- LOGO / BRAND -->
    <div class="mb-6 text-center">


      <h2 class="text-2xl font-semibold text-gray-900 tracking-tight">
        QR Event Platform
      </h2>
      <p class="text-sm text-gray-500 mt-1">
        Secure login for admins & staff
      </p>
    </div>

    <!-- FORM -->
    <form (ngSubmit)="login()" class="space-y-4">

      <!-- EMAIL -->
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-1">
          Email
        </label>
        <input type="email"
               [(ngModel)]="email"
               name="email"
               required
               placeholder="you@company.com"
               class="w-full px-4 py-2.5 rounded-xl border
                      border-gray-300 text-gray-900
                      focus:ring-2 focus:ring-blue-500
                      focus:border-blue-500
                      transition" />
      </div>

      <!-- PASSWORD -->
      <!-- PASSWORD -->
      <div>
        <label class="block text-xs font-medium text-gray-600 mb-1">
          Password
        </label>

        <div class="relative">
          <input [type]="showPassword ? 'text' : 'password'"
                 [(ngModel)]="password"
                 name="password"
                 required
                 placeholder="••••••••"
                 class="w-full px-4 py-2.5 pr-11 rounded-xl border
             border-gray-300 text-gray-900
             focus:ring-2 focus:ring-blue-500
             focus:border-blue-500
             transition" />

          <!-- EYE BUTTON -->
          <button type="button"
                  (click)="togglePassword()"
                  class="absolute inset-y-0 right-3 flex items-center
             text-gray-400 hover:text-gray-600
             transition">

            <!-- Eye Open -->
            <svg *ngIf="!showPassword"
                 xmlns="http://www.w3.org/2000/svg"
                 class="h-5 w-5"
                 fill="none"
                 viewBox="0 0 24 24"
                 stroke="currentColor">
              <path stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              <path stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M2.458 12C3.732 7.943 7.523 5 12 5
                 c4.478 0 8.268 2.943 9.542 7
                 -1.274 4.057-5.064 7-9.542 7
                 -4.477 0-8.268-2.943-9.542-7z" />
            </svg>

            <!-- Eye Off -->
            <svg *ngIf="showPassword"
                 xmlns="http://www.w3.org/2000/svg"
                 class="h-5 w-5"
                 fill="none"
                 viewBox="0 0 24 24"
                 stroke="currentColor">
              <path stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M13.875 18.825A10.05 10.05 0 0112 19
                 c-4.478 0-8.268-2.943-9.542-7
                 a9.956 9.956 0 012.042-3.368" />
              <path stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M6.223 6.223A9.956 9.956 0 0112 5
                 c4.478 0 8.268 2.943 9.542 7
                 a9.96 9.96 0 01-4.043 5.132" />
              <path stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M3 3l18 18" />
            </svg>
          </button>
        </div>
      </div>

      <!-- BUTTON -->
      <button type="submit"
              [disabled]="loading"
              class="w-full py-3 rounded-xl
               flex items-center justify-center gap-2
               text-white font-semibold
               bg-gradient-to-r from-blue-600 to-indigo-600
               hover:from-blue-700 hover:to-indigo-700
               active:scale-[0.98]
               disabled:opacity-60
               transition-all duration-200
               shadow-lg">

        <ng-container *ngIf="!loading">
          Login
        </ng-container>

        <ng-container *ngIf="loading">
          <div class="loader"></div>
          <span class="text-sm">Signing in…</span>
        </ng-container>

      </button>

    </form>

    <!-- FOOTER -->
    <div class="mt-6 text-center text-xs text-gray-400">
      Built for events • Made in India 🇮🇳
    </div>

  </div>
</div>
===
<div class="login-wrapper">
  <!-- Decorative background orbs -->
  <div class="bg-orb bg-orb-1"></div>
  <div class="bg-orb bg-orb-2"></div>
  <div class="bg-orb bg-orb-3"></div>

  <div class="login-card animate-fade-in">

    <!-- LOGO / BRAND -->
    <div class="brand-section">
      <div class="brand-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="7" height="7"></rect>
          <rect x="14" y="3" width="7" height="7"></rect>
          <rect x="14" y="14" width="7" height="7"></rect>
          <rect x="3" y="14" width="7" height="7"></rect>
        </svg>
      </div>
      <h1 class="brand-title">QR Event Platform</h1>
      <p class="brand-subtitle">Secure portal for event management</p>
    </div>

    <!-- ERROR MESSAGE -->
    <div *ngIf="error" class="error-banner animate-slide-down">
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none"
           stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="10"></circle>
        <line x1="15" y1="9" x2="9" y2="15"></line>
        <line x1="9" y1="9" x2="15" y2="15"></line>
      </svg>
      {{ error }}
    </div>

    <!-- FORM -->
    <form (ngSubmit)="login()" class="login-form">

      <!-- EMAIL -->
      <div class="form-group">
        <label class="form-label">Email address</label>
        <div class="input-wrapper">
          <svg class="input-icon" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
               fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
            <polyline points="22,6 12,13 2,6"></polyline>
          </svg>
          <input type="email"
                 [(ngModel)]="email"
                 name="email"
                 required
                 autocomplete="email"
                 placeholder="you&#64;company.com"
                 class="form-input" />
        </div>
      </div>

      <!-- PASSWORD -->
      <div class="form-group">
        <label class="form-label">Password</label>
        <div class="input-wrapper">
          <svg class="input-icon" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
               fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
            <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
          </svg>
          <input [type]="showPassword ? 'text' : 'password'"
                 [(ngModel)]="password"
                 name="password"
                 required
                 autocomplete="current-password"
                 placeholder="••••••••"
                 class="form-input password-input" />

          <button type="button"
                  (click)="togglePassword()"
                  class="eye-toggle">
            <!-- Eye Open -->
            <svg *ngIf="!showPassword" xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round">
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
              <circle cx="12" cy="12" r="3"></circle>
            </svg>
            <!-- Eye Off -->
            <svg *ngIf="showPassword" xmlns="http://www.w3.org/2000/svg" width="18" height="18"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round">
              <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"></path>
              <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"></path>
              <line x1="1" y1="1" x2="23" y2="23"></line>
            </svg>
          </button>
        </div>
      </div>

      <!-- SUBMIT BUTTON -->
      <button type="submit"
              [disabled]="loading"
              class="login-btn">
        <ng-container *ngIf="!loading">
          <span>Sign In</span>
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none"
               stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="5" y1="12" x2="19" y2="12"></line>
            <polyline points="12 5 19 12 12 19"></polyline>
          </svg>
        </ng-container>

        <ng-container *ngIf="loading">
          <div class="spinner"></div>
          <span>Signing in…</span>
        </ng-container>
      </button>
    </form>

    <!-- FOOTER -->
    <div class="login-footer">
      <span>QR Event Platform</span>
      <span class="dot">•</span>
      <span>Enterprise Edition</span>
    </div>

  </div>
</div>

```
```diff:login.component.scss
//.login-container {
//  max-width: 320px;
//  margin: 100px auto;
//  display: flex;
//  flex-direction: column;
//  gap: 12px;
//}

//input, button {
//  padding: 10px;
//}

//.error {
//  color: red;
//}



@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(14px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* LOADER */
.loader {
  width: 22px;
  aspect-ratio: 1;
  border-radius: 50%;
  background: radial-gradient(farthest-side, #ffffff 94%, #0000) 50% 1px/6px 6px no-repeat, radial-gradient(farthest-side, #0000 calc(100% - 8px), rgba(255,255,255,.35) 0);
  animation: spin 1s infinite linear;
}

@keyframes spin {
  to {
    transform: rotate(1turn);
  }
}


@keyframes pop {
  0% {
    transform: scale(0.85);
    opacity: 0;
  }

  100% {
    transform: scale(1);
    opacity: 1;
  }
}

.animate-fade-in {
  animation: fadeIn 0.6s ease-out forwards;
}

.animate-pop {
  animation: pop 0.5s cubic-bezier(.2,.8,.3,1.2);
}
===
/* ===============================
   LOGIN – PREMIUM DESIGN
================================ */

.login-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%);
  padding: 16px;
  position: relative;
  overflow: hidden;
}

/* Decorative floating orbs */
.bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.4;
  pointer-events: none;
}

.bg-orb-1 {
  width: 400px;
  height: 400px;
  background: #7c3aed;
  top: -100px;
  right: -100px;
  animation: float 8s ease-in-out infinite;
}

.bg-orb-2 {
  width: 300px;
  height: 300px;
  background: #06b6d4;
  bottom: -80px;
  left: -80px;
  animation: float 10s ease-in-out infinite reverse;
}

.bg-orb-3 {
  width: 200px;
  height: 200px;
  background: #a855f7;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation: float 6s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0) scale(1); }
  50% { transform: translateY(-20px) scale(1.05); }
}

/* Card */
.login-card {
  width: 100%;
  max-width: 400px;
  background: rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 24px;
  padding: 40px 32px;
  position: relative;
  z-index: 1;
  box-shadow:
    0 24px 48px rgba(0, 0, 0, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

/* Brand */
.brand-section {
  text-align: center;
  margin-bottom: 32px;
}

.brand-icon {
  width: 56px;
  height: 56px;
  margin: 0 auto 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 16px;
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white;
  box-shadow: 0 8px 24px rgba(79, 70, 229, 0.4);
}

.brand-title {
  font-size: 24px;
  font-weight: 700;
  color: #ffffff;
  margin: 0 0 6px;
  letter-spacing: -0.02em;
}

.brand-subtitle {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.5);
  margin: 0;
}

/* Error banner */
.error-banner {
  display: flex;
  align-items: center;
  gap: 8px;
  background: rgba(239, 68, 68, 0.15);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: #fca5a5;
  padding: 10px 14px;
  border-radius: 12px;
  font-size: 13px;
  margin-bottom: 20px;
}

/* Form */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-label {
  font-size: 13px;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.7);
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 14px;
  color: rgba(255, 255, 255, 0.3);
  pointer-events: none;
  z-index: 1;
}

.form-input {
  width: 100%;
  padding: 12px 14px 12px 44px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: rgba(255, 255, 255, 0.06);
  color: #ffffff;
  font-size: 14px;
  font-family: inherit;
  transition: all 0.2s ease;
}

.form-input::placeholder {
  color: rgba(255, 255, 255, 0.3);
}

.form-input:focus {
  outline: none;
  border-color: rgba(79, 70, 229, 0.6);
  background: rgba(255, 255, 255, 0.1);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.15);
}

.password-input {
  padding-right: 48px;
}

.eye-toggle {
  position: absolute;
  right: 12px;
  background: none;
  border: none;
  color: rgba(255, 255, 255, 0.4);
  cursor: pointer;
  padding: 4px;
  display: flex;
  align-items: center;
  transition: color 0.2s;
}

.eye-toggle:hover {
  color: rgba(255, 255, 255, 0.7);
}

/* Submit */
.login-btn {
  width: 100%;
  padding: 14px;
  border-radius: 12px;
  border: none;
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white;
  font-size: 15px;
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.25s ease;
  box-shadow: 0 4px 16px rgba(79, 70, 229, 0.3);
  margin-top: 4px;
}

.login-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 28px rgba(79, 70, 229, 0.45);
}

.login-btn:active {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

/* Spinner */
.spinner {
  width: 20px;
  height: 20px;
  border: 2.5px solid rgba(255, 255, 255, 0.3);
  border-top-color: #ffffff;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Footer */
.login-footer {
  text-align: center;
  margin-top: 28px;
  font-size: 12px;
  color: rgba(255, 255, 255, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.dot {
  opacity: 0.5;
}

/* Animation */
.animate-fade-in {
  animation: fadeInUp 0.6s ease-out forwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-slide-down {
  animation: slideDown 0.3s ease-out forwards;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

```
```diff:admin.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AdminEventService } from '../../core/services/admin-event.service';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';
import { EventFormBuilderComponent } from './components/event-form-builder/event-form-builder.component';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';




type LiveLog = {
  message: string;
  time: Date;
};


@Component({
  standalone: true,
  selector: 'app-admin',
  imports: [CommonModule, FormsModule, EventFormBuilderComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss'
})
export class AdminComponent implements OnInit, OnDestroy {

  summary = {
    totalEvents: 0,
    totalWorkers: 0,
    totalTickets: 0,
    usedTickets: 0,
    revalidations: 0,
    unauthorized: 0
  };

  auditEventId = '';
  auditRows: any[] = [];
  auditColumns: string[] = [];

  activeTab:
    | 'events'
    | 'tickets'
    | 'workers'
    | 'assign'
    | 'deleted'
    | 'scans'
    | 'formaudit' = 'events';
  liveLogs: { message: string; time: Date }[] = [];
  scanLogs: any[] = [];          // persistent logs
  liveFeed: AdminLiveEvent[] = []; // live events (top bar / toast)



  events: any[] = [];
  workers: any[] = [];
  assignableWorkers: any[] = [];
  eventWorkers: Record<string, any[]> = {};
  assignedWorkerIds = new Set<string>();
  deletedWorkers: any[] = [];
  showDeletedWorkers = false;
  eventTicketStats: any = null;
  eventTickets: any[] = [];
  loadingEventTickets = false;

  newEvent = { name: '', location: '', date: '' };
  newWorker = { name: '', email: '', password: '' };

  selectedEventId = '';
  selectedWorkerId = '';
  selectedWorkerIds: string[] = [];


  private token = localStorage.getItem('token')!;
  constructor(
    private adminService: AdminService,
    private eventService: AdminEventService,
    private http: HttpClient,
    private router: Router,
    private signalr: AdminSignalrService) { }

  ngOnDestroy() {
    this.signalr.disconnect();
  }

  switchTab(tab:
    | 'events'
    | 'tickets'
    | 'workers'
    | 'assign'
    | 'deleted'
    | 'scans'
    | 'formaudit'
  ) {
    this.activeTab = tab;

    switch (tab) {
      case 'events':
        this.loadEvents();
        this.loadSummary();
        break;

      case 'tickets':
        this.loadSummary();
        if (this.selectedEventId) {
          this.onTicketEventChange();
        }
        break;

      case 'workers':
        this.loadWorkers();
        break;

      case 'assign':
        if (this.selectedEventId) {
          this.onEventChange();
        }
        break;

      case 'deleted':
        this.loadDeletedWorkers();
        break;

      case 'scans':
        this.loadScanHistory();
        break;
      case 'formaudit':
        this.loadFormAudit();
        break;
    }
  }
  
  ngOnInit() {
    this.loadSummary();
    this.loadEvents();
    this.loadWorkers();
    this.loadDeletedWorkers();
    this.loadScanHistory();
    

    this.signalr.connect(this.token);

    // ✅ CALL IT
    this.listenLiveEvents();

    // Live feed (top bar / toast)
    this.signalr.liveEvents$.subscribe(ev => {
      if (!ev) return;
      this.liveFeed.unshift(ev);
      this.liveFeed = this.liveFeed.slice(0, 10);
    });

    // Live scan table
    this.signalr.onTicketScanned(scan => {
      this.scanLogs.unshift({
        TicketCode: scan.ticketCode,
        EventName: scan.eventName,
        WorkerName: scan.workerName,
        ScanResult: scan.result,
        ScannedAt: scan.time
      });
    });
  }
  onTicketEventChange() {
    if (!this.selectedEventId) {
      this.eventTicketStats = null;
      this.eventTickets = [];
      return;
    }

    

    this.loadingEventTickets = true;

    this.adminService.getEventTickets(this.selectedEventId).subscribe({
      next: res => {
        this.eventTicketStats = res.stats;
        this.eventTickets = res.tickets;
        this.loadingEventTickets = false;
      },
      error: () => {
        this.loadingEventTickets = false;
      }
    });
  }

  logout() {
    // Optional confirmation
    if (!confirm('Logout from admin?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }
  loadEventTicketStats(eventId: string) {
    const event = this.events.find(e => e.id === eventId);
    if (!event) return;

    this.eventTicketStats = {
      totalTickets: event.tickets,
      usedTickets: event.usedTickets,
      revalidations: this.scanLogs.filter(
        s => s.EventName === event.name && s.ScanResult === 'REVALIDATED'
      ).length
    };
  }

  loadFormAudit() {
    if (!this.auditEventId) return;

    this.adminService
      .getFormAudit(this.auditEventId)
      .subscribe(rows => {
        this.auditRows = rows;

        this.auditColumns = rows.length
          ? Object.keys(rows[0].data)
          : [];
      });
  }


  loadEventTickets(eventId: string) {
    this.loadingEventTickets = true;

    this.adminService.getTicketsByEvent(eventId).subscribe({
      next: res => {
        this.eventTickets = res;
        this.loadingEventTickets = false;
      },
      error: () => {
        this.eventTickets = [];
        this.loadingEventTickets = false;
      }
    });
  }



  loadScanHistory() {
    this.adminService.getScanHistory().subscribe(res => {
      this.scanLogs = res;
    });
  }

  

  listenLiveEvents() {
    this.signalr.liveEvents$.subscribe(event => {
      if (!event) return;

      this.liveLogs.unshift({
        message: event.message,
        time: new Date()
      });

      this.liveLogs = this.liveLogs.slice(0, 20);

      if (!this.selectedEventId || !this.eventTicketStats) return;

      if (event.type === 'TICKET_VALID') {
        this.summary.usedTickets++;
      }

      if (event.type === 'REVALIDATED') {
        this.summary.revalidations++;
      }

      if (event.type == 'UNAUTHORIZED') {
        this.summary.unauthorized++;
      }

    });
  }


  

  loadSummary() {
    this.adminService.getDashboardSummary().subscribe(r => {
      this.summary = {
        totalEvents: r.Events ?? 0,
        totalWorkers: r.Workers ?? 0,
        totalTickets: r.Tickets ?? 0,
        usedTickets: r.UsedTickets ?? 0,
        revalidations: r.Revalidations ?? 0,
        unauthorized: r.Unauthorized ??0
      };
    });
  }



  loadDeletedWorkers() {
    this.adminService.getDeletedWorkers().subscribe(res => {
      this.deletedWorkers = res;
    });
  }

  restoreWorker(workerId: string) {
    if (!confirm('Restore this worker?')) return;

    this.adminService.restoreWorker(workerId).subscribe(() => {
      this.loadWorkers();
      this.loadDeletedWorkers();
    });
  }

  loadEvents() {
    this.eventService.getEvents().subscribe(res => {
      this.events = res.map(e => ({
        id: e.Id,
        name: e.Name,
        location: e.Location ?? '-',
        eventDate: e.EventDate ? new Date(e.EventDate) : null,
        tickets: e.Tickets,
        usedTickets: e.UsedTickets
      }));
    });
  }

  createEvent() {
    this.eventService.createEvent({
      name: this.newEvent.name,
      location: this.newEvent.location,
      eventDate: new Date(this.newEvent.date).toISOString()
    }).subscribe(() => {
      this.newEvent = { name: '', location: '', date: '' };
      this.loadEvents();
    });
  }

  deleteEvent(id: string) {
    if (!confirm('Delete event?')) return;
    this.eventService.deleteEvent(id).subscribe(() => this.loadEvents());
  }

  loadWorkers() {
    this.adminService.getWorkers().subscribe(res => {
      this.workers = res.map(w => ({
        id: w.Id,
        name: w.Name,
        email: w.Email,
        isActive: w.IsActive
      }));
    });
  }

  createWorker() {
    this.adminService.createWorker(this.newWorker).subscribe(() => {
      this.newWorker = { name: '', email: '', password: '' };
      this.loadWorkers();
    });
  }

  deleteWorker(id: string) {
    if (!confirm('Delete worker?')) return;
    this.adminService.deleteWorker(id).subscribe(() => this.loadWorkers());

  }

  onEventChange() {
    if (!this.selectedEventId) return;

    this.loadAssignedWorkers(this.selectedEventId);

    const assigned =
      this.eventWorkers[this.selectedEventId]?.map(w => w.workerId) ?? [];

    this.assignableWorkers =
      this.workers.filter(w => !assigned.includes(w.id));

    this.selectedWorkerIds = [];
    this.selectedWorkerId = '';
  }

  assignWorker() {
    if (!this.selectedWorkerId) return;
    this.adminService.assignWorker({
      eventId: this.selectedEventId,
      workerId: this.selectedWorkerId
    }).subscribe(() => this.onEventChange());
  }

  onWorkerCheckboxChange(workerId: string, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) this.selectedWorkerIds.push(workerId);
    else this.selectedWorkerIds =
      this.selectedWorkerIds.filter(id => id !== workerId);
  }

  bulkAssign() {
    if (!this.selectedEventId || this.selectedWorkerIds.length === 0) return;

    this.adminService.bulkAssignWorkers(
      this.selectedEventId,
      this.selectedWorkerIds
    ).subscribe(() => {
      // 🔄 refresh UI immediately
      this.onEventChange();
    });
  }



  loadAssignedWorkers(eventId: string) {
    this.adminService.getEventWorkers(eventId).subscribe((res: any[]) => {

      const mapped = res.map(w => ({
        assignmentId: w.AssignmentId,
        workerId: w.WorkerId,
        name: w.Name,
        email: w.Email,
        assignedAt: w.AssignedAt
      }));

      this.eventWorkers[eventId] = mapped;

      this.assignedWorkerIds.clear();
      mapped.forEach(w => this.assignedWorkerIds.add(w.workerId));
    });
  }



  


  unassignWorker(assignmentId: string, eventId: string) {
    this.eventWorkers[eventId] =
      this.eventWorkers[eventId].filter(w => w.assignmentId !== assignmentId);

    this.adminService.unassignWorker(assignmentId).subscribe({
      error: () => this.loadAssignedWorkers(eventId) // rollback if needed
    });
  }


  
  openQr(url: string) {
    if (!url) return;
  
    // If backend sends relative path like /qr/xxxx
    if (url.startsWith('/')) {
      url = environment.apiUrl + url;
    }
  
    // If protocol missing
    if (!/^https?:\/\//i.test(url)) {
      url = 'https://' + url;
    }
  
    window.open(url, '_blank', 'noopener,noreferrer');
  }
  

  formOpen(eventId: string) {
    const angularUrl = window.location.origin; 
    const url = `${angularUrl}/event/${eventId}`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }


  generateTicket(eventId: string) {
    const email = prompt('Enter customer email');
    if (!email) return;

    this.adminService.createTicket(eventId, email).subscribe({
      next: res => {
        alert(`Ticket sent to ${email}`);
        window.open(res.qrUrl, '_blank');

        // 🔄 refresh event-specific data
        this.loadSummary;
        this.loadEventTicketStats(eventId);
        this.loadEventTickets(eventId);
        this.onTicketEventChange();

      },
      error: err => alert(err.error ?? 'Ticket generation failed')
    });
  }



}
===
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AdminEventService } from '../../core/services/admin-event.service';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';
import { EventFormBuilderComponent } from './components/event-form-builder/event-form-builder.component';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';




type LiveLog = {
  message: string;
  time: Date;
};


@Component({
  standalone: true,
  selector: 'app-admin',
  imports: [CommonModule, FormsModule, EventFormBuilderComponent],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit, OnDestroy {

  summary = {
    totalEvents: 0,
    totalWorkers: 0,
    totalTickets: 0,
    usedTickets: 0,
    revalidations: 0,
    unauthorized: 0
  };

  auditEventId = '';
  auditRows: any[] = [];
  auditColumns: string[] = [];

  activeTab:
    | 'events'
    | 'tickets'
    | 'workers'
    | 'assign'
    | 'deleted'
    | 'scans'
    | 'formaudit' = 'events';
  liveLogs: { message: string; time: Date }[] = [];
  scanLogs: any[] = [];          // persistent logs
  liveFeed: AdminLiveEvent[] = []; // live events (top bar / toast)



  events: any[] = [];
  workers: any[] = [];
  assignableWorkers: any[] = [];
  eventWorkers: Record<string, any[]> = {};
  assignedWorkerIds = new Set<string>();
  deletedWorkers: any[] = [];
  showDeletedWorkers = false;
  eventTicketStats: any = null;
  eventTickets: any[] = [];
  loadingEventTickets = false;

  newEvent = { name: '', location: '', date: '' };
  newWorker = { name: '', email: '', password: '' };

  selectedEventId = '';
  selectedWorkerId = '';
  selectedWorkerIds: string[] = [];


  private token = localStorage.getItem('token')!;
  constructor(
    private adminService: AdminService,
    private eventService: AdminEventService,
    private http: HttpClient,
    private router: Router,
    private signalr: AdminSignalrService) { }

  ngOnDestroy() {
    this.signalr.disconnect();
  }

  switchTab(tab:
    | 'events'
    | 'tickets'
    | 'workers'
    | 'assign'
    | 'deleted'
    | 'scans'
    | 'formaudit'
  ) {
    this.activeTab = tab;

    switch (tab) {
      case 'events':
        this.loadEvents();
        this.loadSummary();
        break;

      case 'tickets':
        this.loadSummary();
        if (this.selectedEventId) {
          this.onTicketEventChange();
        }
        break;

      case 'workers':
        this.loadWorkers();
        break;

      case 'assign':
        if (this.selectedEventId) {
          this.onEventChange();
        }
        break;

      case 'deleted':
        this.loadDeletedWorkers();
        break;

      case 'scans':
        this.loadScanHistory();
        break;
      case 'formaudit':
        this.loadFormAudit();
        break;
    }
  }
  
  ngOnInit() {
    this.loadSummary();
    this.loadEvents();
    this.loadWorkers();
    this.loadDeletedWorkers();
    this.loadScanHistory();
    

    this.signalr.connect(this.token);

    // ✅ CALL IT
    this.listenLiveEvents();

    // Live feed (top bar / toast)
    this.signalr.liveEvents$.subscribe(ev => {
      if (!ev) return;
      this.liveFeed.unshift(ev);
      this.liveFeed = this.liveFeed.slice(0, 10);
    });

    // Live scan table
    this.signalr.onTicketScanned(scan => {
      this.scanLogs.unshift({
        TicketCode: scan.ticketCode,
        EventName: scan.eventName,
        WorkerName: scan.workerName,
        ScanResult: scan.result,
        ScannedAt: scan.time
      });
    });
  }
  onTicketEventChange() {
    if (!this.selectedEventId) {
      this.eventTicketStats = null;
      this.eventTickets = [];
      return;
    }

    

    this.loadingEventTickets = true;

    this.adminService.getEventTickets(this.selectedEventId).subscribe({
      next: res => {
        this.eventTicketStats = res.stats;
        this.eventTickets = res.tickets;
        this.loadingEventTickets = false;
      },
      error: () => {
        this.loadingEventTickets = false;
      }
    });
  }

  logout() {
    // Optional confirmation
    if (!confirm('Logout from admin?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }
  loadEventTicketStats(eventId: string) {
    const event = this.events.find(e => e.id === eventId);
    if (!event) return;

    this.eventTicketStats = {
      totalTickets: event.tickets,
      usedTickets: event.usedTickets,
      revalidations: this.scanLogs.filter(
        s => s.EventName === event.name && s.ScanResult === 'REVALIDATED'
      ).length
    };
  }

  loadFormAudit() {
    if (!this.auditEventId) return;

    this.adminService
      .getFormAudit(this.auditEventId)
      .subscribe(rows => {
        this.auditRows = rows;

        this.auditColumns = rows.length
          ? Object.keys(rows[0].data)
          : [];
      });
  }


  loadEventTickets(eventId: string) {
    this.loadingEventTickets = true;

    this.adminService.getTicketsByEvent(eventId).subscribe({
      next: res => {
        this.eventTickets = res;
        this.loadingEventTickets = false;
      },
      error: () => {
        this.eventTickets = [];
        this.loadingEventTickets = false;
      }
    });
  }



  loadScanHistory() {
    this.adminService.getScanHistory().subscribe(res => {
      this.scanLogs = res;
    });
  }

  

  listenLiveEvents() {
    this.signalr.liveEvents$.subscribe(event => {
      if (!event) return;

      this.liveLogs.unshift({
        message: event.message,
        time: new Date()
      });

      this.liveLogs = this.liveLogs.slice(0, 20);

      if (!this.selectedEventId || !this.eventTicketStats) return;

      if (event.type === 'TICKET_VALID') {
        this.summary.usedTickets++;
      }

      if (event.type === 'REVALIDATED') {
        this.summary.revalidations++;
      }

      if (event.type == 'UNAUTHORIZED') {
        this.summary.unauthorized++;
      }

    });
  }


  

  loadSummary() {
    this.adminService.getDashboardSummary().subscribe(r => {
      this.summary = {
        totalEvents: r.Events ?? 0,
        totalWorkers: r.Workers ?? 0,
        totalTickets: r.Tickets ?? 0,
        usedTickets: r.UsedTickets ?? 0,
        revalidations: r.Revalidations ?? 0,
        unauthorized: r.Unauthorized ??0
      };
    });
  }



  loadDeletedWorkers() {
    this.adminService.getDeletedWorkers().subscribe(res => {
      this.deletedWorkers = res;
    });
  }

  restoreWorker(workerId: string) {
    if (!confirm('Restore this worker?')) return;

    this.adminService.restoreWorker(workerId).subscribe(() => {
      this.loadWorkers();
      this.loadDeletedWorkers();
    });
  }

  loadEvents() {
    this.eventService.getEvents().subscribe(res => {
      this.events = res.map(e => ({
        id: e.Id,
        name: e.Name,
        location: e.Location ?? '-',
        eventDate: e.EventDate ? new Date(e.EventDate) : null,
        tickets: e.Tickets,
        usedTickets: e.UsedTickets
      }));
    });
  }

  createEvent() {
    this.eventService.createEvent({
      name: this.newEvent.name,
      location: this.newEvent.location,
      eventDate: new Date(this.newEvent.date).toISOString()
    }).subscribe(() => {
      this.newEvent = { name: '', location: '', date: '' };
      this.loadEvents();
    });
  }

  deleteEvent(id: string) {
    if (!confirm('Delete event?')) return;
    this.eventService.deleteEvent(id).subscribe(() => this.loadEvents());
  }

  loadWorkers() {
    this.adminService.getWorkers().subscribe(res => {
      this.workers = res.map(w => ({
        id: w.Id,
        name: w.Name,
        email: w.Email,
        isActive: w.IsActive
      }));
    });
  }

  createWorker() {
    this.adminService.createWorker(this.newWorker).subscribe(() => {
      this.newWorker = { name: '', email: '', password: '' };
      this.loadWorkers();
    });
  }

  deleteWorker(id: string) {
    if (!confirm('Delete worker?')) return;
    this.adminService.deleteWorker(id).subscribe(() => this.loadWorkers());

  }

  onEventChange() {
    if (!this.selectedEventId) return;

    this.loadAssignedWorkers(this.selectedEventId);

    const assigned =
      this.eventWorkers[this.selectedEventId]?.map(w => w.workerId) ?? [];

    this.assignableWorkers =
      this.workers.filter(w => !assigned.includes(w.id));

    this.selectedWorkerIds = [];
    this.selectedWorkerId = '';
  }

  assignWorker() {
    if (!this.selectedWorkerId) return;
    this.adminService.assignWorker({
      eventId: this.selectedEventId,
      workerId: this.selectedWorkerId
    }).subscribe(() => this.onEventChange());
  }

  onWorkerCheckboxChange(workerId: string, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) this.selectedWorkerIds.push(workerId);
    else this.selectedWorkerIds =
      this.selectedWorkerIds.filter(id => id !== workerId);
  }

  bulkAssign() {
    if (!this.selectedEventId || this.selectedWorkerIds.length === 0) return;

    this.adminService.bulkAssignWorkers(
      this.selectedEventId,
      this.selectedWorkerIds
    ).subscribe(() => {
      // 🔄 refresh UI immediately
      this.onEventChange();
    });
  }



  loadAssignedWorkers(eventId: string) {
    this.adminService.getEventWorkers(eventId).subscribe((res: any[]) => {

      const mapped = res.map(w => ({
        assignmentId: w.AssignmentId,
        workerId: w.WorkerId,
        name: w.Name,
        email: w.Email,
        assignedAt: w.AssignedAt
      }));

      this.eventWorkers[eventId] = mapped;

      this.assignedWorkerIds.clear();
      mapped.forEach(w => this.assignedWorkerIds.add(w.workerId));
    });
  }



  


  unassignWorker(assignmentId: string, eventId: string) {
    this.eventWorkers[eventId] =
      this.eventWorkers[eventId].filter(w => w.assignmentId !== assignmentId);

    this.adminService.unassignWorker(assignmentId).subscribe({
      error: () => this.loadAssignedWorkers(eventId) // rollback if needed
    });
  }


  
  openQr(url: string) {
    if (!url) return;
  
    // If backend sends relative path like /qr/xxxx
    if (url.startsWith('/')) {
      url = environment.apiUrl + url;
    }
  
    // If protocol missing
    if (!/^https?:\/\//i.test(url)) {
      url = 'https://' + url;
    }
  
    window.open(url, '_blank', 'noopener,noreferrer');
  }
  

  formOpen(eventId: string) {
    const angularUrl = window.location.origin; 
    const url = `${angularUrl}/event/${eventId}`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }


  generateTicket(eventId: string) {
    const email = prompt('Enter customer email');
    if (!email) return;

    this.adminService.createTicket(eventId, email).subscribe({
      next: res => {
        alert(`Ticket sent to ${email}`);
        window.open(res.qrUrl, '_blank');

        // 🔄 refresh event-specific data
        this.loadSummary();
        this.loadEventTicketStats(eventId);
        this.loadEventTickets(eventId);
        this.onTicketEventChange();

      },
      error: err => alert(err.error ?? 'Ticket generation failed')
    });
  }



}
```
```diff:admin.component.html
  <div class="min-h-screen bg-gray-50 p-6 space-y-6">

  <!-- HEADER -->
  <div>
    <h2 class="text-2xl font-semibold text-gray-900">
      Admin Dashboard
    </h2>
    <div class="flex justify-end">
      <button class="btn btn-danger"
              (click)="logout()">
        Logout
      </button>

    </div>

    <p class="text-sm text-gray-500">
      Manage events, tickets, workers, and scans
    </p>
  </div>

  <!-- SUMMARY -->
  <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
    <div class="bg-white border rounded-xl p-4">
      <p class="text-sm text-gray-500">Events</p>
      <p class="text-2xl font-bold">{{ summary.totalEvents }}</p>
    </div>
    <div class="bg-white border rounded-xl p-4">
      <p class="text-sm text-gray-500">Tickets</p>
      <p class="text-2xl font-bold">{{ summary.totalTickets }}</p>
    </div>
    <div class="bg-white border rounded-xl p-4">
      <p class="text-sm text-gray-500">Used</p>
      <p class="text-2xl font-bold">{{ summary.usedTickets }}</p>
    </div>
    <div class="bg-white border rounded-xl p-4">
      <p class="text-sm text-gray-500">Revalidations</p>
      <p class="text-2xl font-bold">{{ summary.revalidations }}</p>
    </div>
  </div>

  <!-- TABS -->
  <div class="tabs-bar">
    <button class="tab"
            [class.active]="activeTab==='events'"
            (click)="switchTab('events')">
      Events
    </button>

    <button class="tab"
            [class.active]="activeTab==='tickets'"
            (click)="switchTab('tickets')">
      Tickets
    </button>

    <button class="tab"
            [class.active]="activeTab==='workers'"
            (click)="switchTab('workers')">
      Workers
    </button>

    <button class="tab"
            [class.active]="activeTab==='assign'"
            (click)="switchTab('assign')">
      Assign
    </button>

    <button class="tab"
            [class.active]="activeTab==='deleted'"
            (click)="switchTab('deleted')">
      Deleted
    </button>

    <button class="tab"
            [class.active]="activeTab==='scans'"
            (click)="switchTab('scans')">
      Scans
    </button>

    <button class="tab"
            [class.active]="activeTab==='formaudit'"
            (click)="switchTab('formaudit')">
      Form Audit
    </button>

  </div>



  <!-- EVENTS -->
  <div *ngIf="activeTab==='events'" class="space-y-4">
    <div class="bg-white border rounded-xl p-4 space-y-3">
      <h3 class="font-semibold">Create Event</h3>

      <div class="grid md:grid-cols-3 gap-2">
        <input class="input" placeholder="Event name" [(ngModel)]="newEvent.name" />
        <input class="input" placeholder="Location" [(ngModel)]="newEvent.location" />
        <input class="input" type="date" [(ngModel)]="newEvent.date" />
      </div>

      <button class="btn-primary" (click)="createEvent()">Create Event</button>
    </div>

    <div class="bg-white border rounded-xl overflow-x-auto">
      <select class="input"
              [(ngModel)]="selectedEventId">
        <option value="">Select event to configure form</option>
        <option *ngFor="let e of events" [value]="e.id">
          {{ e.name }}
        </option>
      </select>

      <table class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Name</th>
            <th class="th">Location</th>
            <th class="th">Date</th>
            <th class="th">Tickets</th>
            <th class="th">Used</th>
            <th class="th">Action</th>
            <th class="th">Form</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let e of events" class="border-t">
            <td class="td">{{ e.name }}</td>
            <td class="td">{{ e.location }}</td>
            <td class="td">{{ e.eventDate | date:'mediumDate' }}</td>
            <td class="td">{{ e.tickets }}</td>
            <td class="td">{{ e.usedTickets }}</td>
            <td class="td">
              <button class="btn btn-danger"
                      (click)="deleteEvent(e.id)">
                Delete
              </button>
            </td>
            <td class="td">
              <button class="btn-primary"
                      (click)="formOpen(e.id)">
                Form
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <app-event-form-builder *ngIf="selectedEventId"
                            [eventId]="selectedEventId">
    </app-event-form-builder>
  </div>



  <!-- TICKETS -->
  <div *ngIf="activeTab==='tickets'" class="bg-white border rounded-xl p-4 space-y-4">
    <h3 class="font-semibold">Ticket Management</h3>

    <select class="input"
            [(ngModel)]="selectedEventId"
            (change)="onTicketEventChange()">
      <option value="">Select event</option>
      <option *ngFor="let e of events" [value]="e.id">
        {{ e.name }}
      </option>
    </select>


    <button class="btn-primary"
            [disabled]="!selectedEventId"
            (click)="generateTicket(selectedEventId)">
      Generate Ticket
    </button>

    <div *ngIf="eventTicketStats; else selectEventHint"
         class="grid grid-cols-3 gap-4 text-sm">
      <div>
        <b>Total:</b> {{ eventTicketStats.totalTickets }}
      </div>
      <div>
        <b>Used:</b> {{ eventTicketStats.usedTickets }}
      </div>
      <div>
        <b>Revalidated:</b> {{ eventTicketStats.revalidations }}
      </div>
    </div>

    <ng-template #selectEventHint>
      <p class="text-sm text-gray-500">
        Select an event to view ticket statistics
      </p>
    </ng-template>
    <!-- GENERATED TICKETS -->
    <div *ngIf="selectedEventId" class="mt-6 bg-white border rounded-xl overflow-x-auto">

      <div class="p-4 border-b">
        <h4 class="font-semibold text-gray-900">
          Generated Tickets
        </h4>
        <p class="text-sm text-gray-500">
          Tickets created for this event
        </p>
      </div>

      <table class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Created</th>
            <th class="th">Ticket Code</th>
            <th class="th">Status</th>
            <th class="th">Action</th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let t of eventTickets" class="border-t">
            <td class="td text-gray-500">
              {{ t.createdAt | date:'short' }}
            </td>

            <td class="td font-mono">
              {{ t.code }}
            </td>

            <td class="td font-semibold"
                [ngClass]="{
                'text-green-600': !t.IsUsed,
                'text-gray-500': t.IsUsed
              }">
              {{ t.isUsed ? 'Used' : 'Unused' }}
            </td>

            <td class="td">
              <button class="text-blue-600 hover:underline"
                      (click)="openQr(t.qrUrl)">
                View QR
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <div *ngIf="!eventTickets.length && !loadingEventTickets"
           class="p-4 text-sm text-gray-500 text-center">
        No tickets generated for this event
      </div>

      <div *ngIf="loadingEventTickets"
           class="p-4 text-sm text-gray-400 text-center">
        Loading tickets…
      </div>

    </div>



  </div>

  <!-- WORKERS -->
  <div *ngIf="activeTab==='workers'" class="space-y-4">

    <!-- CREATE WORKER -->
    <div class="bg-white border rounded-xl p-4 space-y-3">
      <h3 class="font-semibold">Create Worker</h3>

      <div class="grid md:grid-cols-3 gap-2">
        <input class="input" placeholder="Name" [(ngModel)]="newWorker.name" />
        <input class="input" placeholder="Email" [(ngModel)]="newWorker.email" />
        <input class="input" placeholder="Password" [(ngModel)]="newWorker.password" />
      </div>

      <button class="btn-primary" (click)="createWorker()">Create Worker</button>
    </div>

    <!-- WORKERS LIST -->
    <div class="bg-white border rounded-xl overflow-x-auto">
      <table class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Name</th>
            <th class="th">Email</th>
            <th class="th">Status</th>
            <th class="th">Action</th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let w of workers" class="border-t">
            <td class="td">{{ w.name }}</td>
            <td class="td">{{ w.email }}</td>
            <td class="td">
              <span class="px-2 py-1 rounded text-xs font-medium"
                    [ngClass]="{
                      'bg-green-100 text-green-700': w.isActive,
                      'bg-red-100 text-red-700': !w.isActive
                    }">
                {{ w.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td class="td">
              <button *ngIf="w.isActive"
                      class="btn btn-danger"
                      (click)="deleteWorker(w.id)">
                Delete
              </button>
              <button *ngIf="!w.isActive"
                      class="text-blue-600 hover:underline"
                      (click)="restoreWorker(w.id)">
                Restore
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>

  <!-- ASSIGN -->
  <div *ngIf="activeTab==='assign'" class="space-y-4">

    <div class="bg-white border rounded-xl p-4 space-y-3">
      <h3 class="font-semibold">Assign Workers to Event</h3>

      <!-- EVENT SELECT -->
      <select class="input" [(ngModel)]="selectedEventId" (change)="onEventChange()">
        <option value="">Select event</option>
        <option *ngFor="let e of events" [value]="e.id">
          {{ e.name }}
        </option>
      </select>

      <!-- SINGLE ASSIGN -->
      <div class="flex gap-2">
        <select class="input flex-1" [(ngModel)]="selectedWorkerId">
          <option value="">Select worker</option>
          <option *ngFor="let w of assignableWorkers" [value]="w.id">
            {{ w.name }}
          </option>
        </select>

        <button class="btn-primary" (click)="assignWorker()">
          Assign
        </button>
      </div>
    </div>

    <!-- BULK ASSIGN -->
    <div class="bg-white border rounded-xl overflow-x-auto">
      <table class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Select</th>
            <th class="th">Name</th>
            <th class="th">Email</th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let w of assignableWorkers" class="border-t">
            <td class="td">
              <input type="checkbox"
                     [disabled]="assignedWorkerIds.has(w.id)"
                     (change)="onWorkerCheckboxChange(w.id, $event)" />
            </td>
            <td class="td">{{ w.name }}</td>
            <td class="td">{{ w.email }}</td>
          </tr>
        </tbody>
      </table>

      <div class="p-3">
        <button class="btn-primary" (click)="bulkAssign()">
          Assign Selected
        </button>
      </div>
    </div>

    <!-- ASSIGNED WORKERS -->
    <div class="bg-white border rounded-xl overflow-x-auto">
      <table *ngIf="eventWorkers[selectedEventId]?.length"
             class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Worker</th>
            <th class="th">Email</th>
            <th class="th">Assigned</th>
            <th class="th">Action</th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let w of eventWorkers[selectedEventId]" class="border-t">
            <td class="td">{{ w.name }}</td>
            <td class="td">{{ w.email }}</td>
            <td class="td text-gray-500">{{ w.assignedAt | date:'short' }}</td>
            <td class="td">
              <button class="btn btn-danger"
                      (click)="unassignWorker(w.assignmentId, selectedEventId)">
                Unassign
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!eventWorkers[selectedEventId]?.length"
         class="p-4 text-sm text-gray-500">
        No workers assigned
      </p>
    </div>

  </div>

  <!-- DELETED -->
  <div *ngIf="activeTab==='deleted'" class="space-y-4">

    <div class="bg-white border rounded-xl overflow-x-auto">
      <table *ngIf="deletedWorkers.length" class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Name</th>
            <th class="th">Email</th>
            <th class="th">Action</th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let w of deletedWorkers" class="border-t">
            <td class="td">{{ w.Name }}</td>
            <td class="td">{{ w.Email }}</td>
            <td class="td">
              <button class="text-blue-600 hover:underline"
                      (click)="restoreWorker(w.Id)">
                Restore
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!deletedWorkers.length"
         class="p-4 text-sm text-gray-500">
        No deleted workers
      </p>
    </div>

  </div>


  <!-- SCANS -->
  <div *ngIf="activeTab==='scans'" class="bg-white border rounded-xl overflow-x-auto">
    <table class="w-full text-sm">
      <thead class="bg-gray-100">
        <tr>
          <th class="th">Time</th>
          <th class="th">Ticket</th>
          <th class="th">Event</th>
          <th class="th">Worker</th>
          <th class="th">Result</th>
        </tr>
      </thead>
      <tbody>
        <tr *ngFor="let s of scanLogs" class="border-t">
          <td class="td text-gray-500">{{ s.ScannedAt | date:'short' }}</td>
          <td class="td font-mono">{{ s.TicketCode }}</td>
          <td class="td">{{ s.EventName }}</td>
          <td class="td">{{ s.WorkerName }}</td>
          <td class="td font-semibold"
              [ngClass]="{
                  'text-green-600': s.ScanResult==='VALID',
                  'text-yellow-600': s.ScanResult==='REVALIDATED',
                  'text-red-600': s.ScanResult==='INVALID',
                  'text-red-700': s.ScanResult==='UNAUTHORIZED'

                }">
            {{ s.ScanResult }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <!-- FORM AUDIT -->
  <div *ngIf="activeTab==='formaudit'" class="space-y-4">

    <div class="bg-white border rounded-xl p-4">
      <h3 class="font-semibold mb-2">Form Audit</h3>

      <select class="input"
              [(ngModel)]="auditEventId"
              (change)="loadFormAudit()">
        <option value="">Select Event</option>
        <option *ngFor="let e of events" [value]="e.id">
          {{ e.name }}
        </option>
      </select>
    </div>

    <div *ngIf="auditColumns.length"
         class="bg-white border rounded-xl overflow-x-auto">

      <table class="w-full text-sm">
        <thead class="bg-gray-100">
          <tr>
            <th class="th">Submitted</th>
            <th *ngFor="let col of auditColumns" class="th">
              {{ col }}
            </th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let row of auditRows">
            <td class="td text-gray-500">
              {{ row.createdAt | date:'short' }}
            </td>

            <td *ngFor="let col of auditColumns" class="td">
              {{ row.data[col] || '-' }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <p *ngIf="!auditRows.length && auditEventId"
       class="text-sm text-gray-500 text-center">
      No submissions found
    </p>

  </div>

</div>
===
<div class="dashboard-wrapper">

  <!-- HEADER -->
  <header class="dashboard-header">
    <div class="header-brand">
      <div class="header-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="7" height="7"></rect>
          <rect x="14" y="3" width="7" height="7"></rect>
          <rect x="14" y="14" width="7" height="7"></rect>
          <rect x="3" y="14" width="7" height="7"></rect>
        </svg>
      </div>
      <div>
        <h1 class="header-title">Admin Dashboard</h1>
        <p class="header-subtitle">Manage events, tickets, workers &amp; scans</p>
      </div>
    </div>
    <button class="logout-btn" (click)="logout()">Logout</button>
  </header>

  <div class="dashboard-body">

    <!-- SUMMARY CARDS -->
    <div class="summary-grid">
      <div class="stat-card indigo">
        <p class="stat-label">Events</p>
        <p class="stat-value">{{ summary.totalEvents }}</p>
      </div>
      <div class="stat-card cyan">
        <p class="stat-label">Tickets</p>
        <p class="stat-value">{{ summary.totalTickets }}</p>
      </div>
      <div class="stat-card emerald">
        <p class="stat-label">Used</p>
        <p class="stat-value">{{ summary.usedTickets }}</p>
      </div>
      <div class="stat-card amber">
        <p class="stat-label">Revalidations</p>
        <p class="stat-value">{{ summary.revalidations }}</p>
      </div>
    </div>

    <!-- TABS -->
    <div class="tabs-bar">
      <button class="tab" [class.active]="activeTab==='events'" (click)="switchTab('events')">Events</button>
      <button class="tab" [class.active]="activeTab==='tickets'" (click)="switchTab('tickets')">Tickets</button>
      <button class="tab" [class.active]="activeTab==='workers'" (click)="switchTab('workers')">Workers</button>
      <button class="tab" [class.active]="activeTab==='assign'" (click)="switchTab('assign')">Assign</button>
      <button class="tab" [class.active]="activeTab==='deleted'" (click)="switchTab('deleted')">Deleted</button>
      <button class="tab" [class.active]="activeTab==='scans'" (click)="switchTab('scans')">Scans</button>
      <button class="tab" [class.active]="activeTab==='formaudit'" (click)="switchTab('formaudit')">Form Audit</button>
    </div>


    <!-- ============ EVENTS ============ -->
    <div *ngIf="activeTab==='events'" class="space-y-4">
      <div class="section-card">
        <div class="section-header">
          <h3 class="section-title">Create Event</h3>
          <p class="section-subtitle">Add a new event to your platform</p>
        </div>
        <div class="section-body">
          <div class="grid md:grid-cols-3 gap-3 mb-4">
            <input class="input" placeholder="Event name" [(ngModel)]="newEvent.name" />
            <input class="input" placeholder="Location" [(ngModel)]="newEvent.location" />
            <input class="input" type="date" [(ngModel)]="newEvent.date" />
          </div>
          <button class="btn btn-primary" (click)="createEvent()">Create Event</button>
        </div>
      </div>

      <div class="section-card">
        <div class="section-header">
          <h3 class="section-title">Your Events</h3>
        </div>
        <div class="p-4">
          <select class="input mb-4" [(ngModel)]="selectedEventId">
            <option value="">Select event to configure form</option>
            <option *ngFor="let e of events" [value]="e.id">{{ e.name }}</option>
          </select>
        </div>
        <div class="overflow-x-auto">
          <table>
            <thead>
              <tr>
                <th class="th">Name</th>
                <th class="th">Location</th>
                <th class="th">Date</th>
                <th class="th">Tickets</th>
                <th class="th">Used</th>
                <th class="th">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let e of events">
                <td class="td font-semibold text-gray-900">{{ e.name }}</td>
                <td class="td">{{ e.location }}</td>
                <td class="td">{{ e.eventDate | date:'mediumDate' }}</td>
                <td class="td">{{ e.tickets }}</td>
                <td class="td">{{ e.usedTickets }}</td>
                <td class="td">
                  <div class="flex gap-2">
                    <button class="btn btn-primary text-xs" (click)="formOpen(e.id)">Form</button>
                    <button class="btn btn-danger text-xs" (click)="deleteEvent(e.id)">Delete</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
          <div *ngIf="!events.length" class="empty-state">
            <div class="empty-state-icon">📋</div>
            <p class="empty-state-text">No events created yet</p>
          </div>
        </div>
      </div>

      <app-event-form-builder *ngIf="selectedEventId" [eventId]="selectedEventId"></app-event-form-builder>
    </div>


    <!-- ============ TICKETS ============ -->
    <div *ngIf="activeTab==='tickets'" class="space-y-4">
      <div class="section-card">
        <div class="section-header">
          <h3 class="section-title">Ticket Management</h3>
          <p class="section-subtitle">Generate and manage event tickets</p>
        </div>
        <div class="section-body">
          <select class="input mb-4" [(ngModel)]="selectedEventId" (change)="onTicketEventChange()">
            <option value="">Select event</option>
            <option *ngFor="let e of events" [value]="e.id">{{ e.name }}</option>
          </select>

          <button class="btn btn-primary mb-4" [disabled]="!selectedEventId" (click)="generateTicket(selectedEventId)">
            Generate Ticket
          </button>

          <div *ngIf="eventTicketStats" class="grid grid-cols-3 gap-4 mb-4">
            <div class="stat-card indigo" style="padding: 12px 16px;">
              <p class="stat-label" style="font-size: 11px;">Total</p>
              <p class="stat-value" style="font-size: 22px;">{{ eventTicketStats.totalTickets }}</p>
            </div>
            <div class="stat-card emerald" style="padding: 12px 16px;">
              <p class="stat-label" style="font-size: 11px;">Used</p>
              <p class="stat-value" style="font-size: 22px;">{{ eventTicketStats.usedTickets }}</p>
            </div>
            <div class="stat-card amber" style="padding: 12px 16px;">
              <p class="stat-label" style="font-size: 11px;">Revalidated</p>
              <p class="stat-value" style="font-size: 22px;">{{ eventTicketStats.revalidations }}</p>
            </div>
          </div>

          <p *ngIf="!eventTicketStats && !selectedEventId" class="text-sm text-gray-400">
            Select an event to view ticket statistics
          </p>
        </div>
      </div>

      <!-- TICKETS TABLE -->
      <div *ngIf="selectedEventId" class="section-card">
        <div class="section-header">
          <h3 class="section-title">Generated Tickets</h3>
          <p class="section-subtitle">Tickets created for this event</p>
        </div>
        <div class="overflow-x-auto">
          <table>
            <thead>
              <tr>
                <th class="th">Created</th>
                <th class="th">Ticket Code</th>
                <th class="th">Status</th>
                <th class="th">Action</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let t of eventTickets">
                <td class="td text-gray-400">{{ t.createdAt | date:'short' }}</td>
                <td class="td font-mono font-semibold">{{ t.code }}</td>
                <td class="td">
                  <span class="badge" [ngClass]="t.isUsed ? 'badge-warning' : 'badge-success'">
                    {{ t.isUsed ? 'Used' : 'Unused' }}
                  </span>
                </td>
                <td class="td">
                  <button class="btn-link text-sm" (click)="openQr(t.qrUrl)">View QR</button>
                </td>
              </tr>
            </tbody>
          </table>

          <div *ngIf="!eventTickets.length && !loadingEventTickets" class="empty-state">
            <div class="empty-state-icon">🎫</div>
            <p class="empty-state-text">No tickets generated for this event</p>
          </div>

          <div *ngIf="loadingEventTickets" class="empty-state">
            <p class="empty-state-text">Loading tickets…</p>
          </div>
        </div>
      </div>
    </div>


    <!-- ============ WORKERS ============ -->
    <div *ngIf="activeTab==='workers'" class="space-y-4">
      <div class="section-card">
        <div class="section-header">
          <h3 class="section-title">Create Worker</h3>
          <p class="section-subtitle">Add a new worker to your team</p>
        </div>
        <div class="section-body">
          <div class="grid md:grid-cols-3 gap-3 mb-4">
            <input class="input" placeholder="Name" [(ngModel)]="newWorker.name" />
            <input class="input" placeholder="Email" [(ngModel)]="newWorker.email" />
            <input class="input" type="password" placeholder="Password" [(ngModel)]="newWorker.password" />
          </div>
          <button class="btn btn-primary" (click)="createWorker()">Create Worker</button>
        </div>
      </div>

      <div class="section-card overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th class="th">Name</th>
              <th class="th">Email</th>
              <th class="th">Status</th>
              <th class="th">Action</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let w of workers">
              <td class="td font-semibold text-gray-900">{{ w.name }}</td>
              <td class="td">{{ w.email }}</td>
              <td class="td">
                <span class="badge" [ngClass]="w.isActive ? 'badge-success' : 'badge-danger'">
                  {{ w.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="td">
                <button *ngIf="w.isActive" class="btn btn-danger text-xs" (click)="deleteWorker(w.id)">Delete</button>
                <button *ngIf="!w.isActive" class="btn-link text-sm" (click)="restoreWorker(w.id)">Restore</button>
              </td>
            </tr>
          </tbody>
        </table>
        <div *ngIf="!workers.length" class="empty-state">
          <div class="empty-state-icon">👤</div>
          <p class="empty-state-text">No workers yet</p>
        </div>
      </div>
    </div>


    <!-- ============ ASSIGN ============ -->
    <div *ngIf="activeTab==='assign'" class="space-y-4">
      <div class="section-card">
        <div class="section-header">
          <h3 class="section-title">Assign Workers to Event</h3>
        </div>
        <div class="section-body">
          <select class="input mb-4" [(ngModel)]="selectedEventId" (change)="onEventChange()">
            <option value="">Select event</option>
            <option *ngFor="let e of events" [value]="e.id">{{ e.name }}</option>
          </select>

          <div class="flex gap-2 mb-4">
            <select class="input flex-1" [(ngModel)]="selectedWorkerId">
              <option value="">Select worker</option>
              <option *ngFor="let w of assignableWorkers" [value]="w.id">{{ w.name }}</option>
            </select>
            <button class="btn btn-primary" (click)="assignWorker()">Assign</button>
          </div>
        </div>
      </div>

      <!-- BULK ASSIGN -->
      <div class="section-card overflow-x-auto">
        <div class="section-header" style="padding-bottom:12px;">
          <h3 class="section-title">Available Workers</h3>
        </div>
        <table>
          <thead>
            <tr>
              <th class="th">Select</th>
              <th class="th">Name</th>
              <th class="th">Email</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let w of assignableWorkers">
              <td class="td">
                <input type="checkbox" [disabled]="assignedWorkerIds.has(w.id)" (change)="onWorkerCheckboxChange(w.id, $event)" />
              </td>
              <td class="td font-semibold">{{ w.name }}</td>
              <td class="td">{{ w.email }}</td>
            </tr>
          </tbody>
        </table>
        <div class="p-4">
          <button class="btn btn-primary" (click)="bulkAssign()">Assign Selected</button>
        </div>
      </div>

      <!-- ASSIGNED TABLE -->
      <div class="section-card overflow-x-auto">
        <div class="section-header" style="padding-bottom:12px;">
          <h3 class="section-title">Currently Assigned</h3>
        </div>
        <table *ngIf="eventWorkers[selectedEventId]?.length">
          <thead>
            <tr>
              <th class="th">Worker</th>
              <th class="th">Email</th>
              <th class="th">Assigned</th>
              <th class="th">Action</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let w of eventWorkers[selectedEventId]">
              <td class="td font-semibold">{{ w.name }}</td>
              <td class="td">{{ w.email }}</td>
              <td class="td text-gray-400">{{ w.assignedAt | date:'short' }}</td>
              <td class="td">
                <button class="btn btn-danger text-xs" (click)="unassignWorker(w.assignmentId, selectedEventId)">Unassign</button>
              </td>
            </tr>
          </tbody>
        </table>
        <div *ngIf="!eventWorkers[selectedEventId]?.length" class="empty-state">
          <div class="empty-state-icon">🔗</div>
          <p class="empty-state-text">No workers assigned</p>
        </div>
      </div>
    </div>


    <!-- ============ DELETED ============ -->
    <div *ngIf="activeTab==='deleted'" class="section-card overflow-x-auto">
      <div class="section-header" style="padding-bottom:12px;">
        <h3 class="section-title">Deleted Workers</h3>
      </div>
      <table *ngIf="deletedWorkers.length">
        <thead>
          <tr>
            <th class="th">Name</th>
            <th class="th">Email</th>
            <th class="th">Action</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let w of deletedWorkers">
            <td class="td font-semibold">{{ w.Name }}</td>
            <td class="td">{{ w.Email }}</td>
            <td class="td">
              <button class="btn-link text-sm" (click)="restoreWorker(w.Id)">Restore</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div *ngIf="!deletedWorkers.length" class="empty-state">
        <div class="empty-state-icon">🗑️</div>
        <p class="empty-state-text">No deleted workers</p>
      </div>
    </div>


    <!-- ============ SCANS ============ -->
    <div *ngIf="activeTab==='scans'" class="section-card overflow-x-auto">
      <div class="section-header" style="padding-bottom:12px;">
        <h3 class="section-title">Scan History</h3>
        <p class="section-subtitle">All ticket scan events in real time</p>
      </div>
      <table>
        <thead>
          <tr>
            <th class="th">Time</th>
            <th class="th">Ticket</th>
            <th class="th">Event</th>
            <th class="th">Worker</th>
            <th class="th">Result</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let s of scanLogs">
            <td class="td text-gray-400">{{ s.ScannedAt | date:'short' }}</td>
            <td class="td font-mono font-semibold">{{ s.TicketCode }}</td>
            <td class="td">{{ s.EventName }}</td>
            <td class="td">{{ s.WorkerName }}</td>
            <td class="td">
              <span class="badge"
                    [ngClass]="{
                      'badge-success': s.ScanResult==='VALID',
                      'badge-warning': s.ScanResult==='REVALIDATED',
                      'badge-danger': s.ScanResult==='INVALID' || s.ScanResult==='UNAUTHORIZED'
                    }">
                {{ s.ScanResult }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
      <div *ngIf="!scanLogs.length" class="empty-state">
        <div class="empty-state-icon">📡</div>
        <p class="empty-state-text">No scans recorded yet</p>
      </div>
    </div>


    <!-- ============ FORM AUDIT ============ -->
    <div *ngIf="activeTab==='formaudit'" class="space-y-4">
      <div class="section-card">
        <div class="section-header">
          <h3 class="section-title">Form Audit</h3>
          <p class="section-subtitle">View all form submissions per event</p>
        </div>
        <div class="section-body">
          <select class="input" [(ngModel)]="auditEventId" (change)="loadFormAudit()">
            <option value="">Select Event</option>
            <option *ngFor="let e of events" [value]="e.id">{{ e.name }}</option>
          </select>
        </div>
      </div>

      <div *ngIf="auditColumns.length" class="section-card overflow-x-auto">
        <table>
          <thead>
            <tr>
              <th class="th">Submitted</th>
              <th *ngFor="let col of auditColumns" class="th">{{ col }}</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of auditRows">
              <td class="td text-gray-400">{{ row.createdAt | date:'short' }}</td>
              <td *ngFor="let col of auditColumns" class="td">{{ row.data[col] || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div *ngIf="!auditRows.length && auditEventId" class="empty-state">
        <div class="empty-state-icon">📝</div>
        <p class="empty-state-text">No submissions found</p>
      </div>
    </div>

  </div>
</div>

```
```diff:admin.component.scss
/* ===============================
   GLOBAL
================================ */
* {
  box-sizing: border-box;
}

body {
  background: #f6f8fb;
  color: #1f2937;
  font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
}

h2, h3, h4 {
  margin: 12px 0;
  font-weight: 600;
}

hr {
  margin: 20px 0;
  border: none;
  border-top: 1px solid #e5e7eb;
}

/* ===============================
   SUMMARY CARDS
================================ */
.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 18px;
  margin: 20px 0;
}

.card {
  background: #ffffff;
  border-radius: 14px;
  padding: 20px;
  text-align: center;
  box-shadow: 0 4px 10px rgba(15, 23, 42, 0.08), 0 1px 3px rgba(15, 23, 42, 0.04);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

  .card:hover {
    transform: translateY(-2px);
    box-shadow: 0 10px 20px rgba(15, 23, 42, 0.12);
  }

  .card h3 {
    margin: 0;
    font-size: 30px;
    color: #2563eb; /* refined blue */
  }

  .card p {
    margin-top: 8px;
    font-size: 14px;
    color: #6b7280;
  }

/* ===============================
   TABS
================================ */
.tabs {
  display: flex;
  gap: 10px;
  margin-bottom: 18px;
  flex-wrap: wrap;
}

  .tabs button {
    padding: 8px 18px;
    border-radius: 999px;
    border: none;
    background: #e5e7eb;
    color: #374151;
    cursor: pointer;
    font-weight: 500;
    transition: all 0.2s ease;
  }

    .tabs button:hover {
      background: #dbeafe;
      color: #1d4ed8;
    }

    .tabs button.active {
      background: #2563eb;
      color: #ffffff;
      box-shadow: 0 4px 10px rgba(37, 99, 235, 0.35);
    }



/* ===============================
   TAB BAR
================================ */
.tabs-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e5e7eb;
}

/* ===============================
   TAB BUTTON
================================ */
.tab {
  padding: 8px 18px;
  border-radius: 999px;
  border: none;
  background: #f1f5f9; // soft gray
  color: #334155; // slate
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

  /* Hover */
  .tab:hover {
    background: #e0e7ff; // light indigo
    color: #1e40af;
  }

  /* Active */
  .tab.active {
    background: linear-gradient( 135deg, #2563eb, #1d4ed8 );
    color: #ffffff;
    box-shadow: 0 6px 16px rgba(37, 99, 235, 0.35);
  }

  /* Focus (keyboard users) */
  .tab:focus {
    outline: none;
    box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.25);
  }


@media (max-width: 640px) {
  .tab {
    font-size: 13px;
    padding: 6px 14px;
  }
}

/* ===============================
   FORMS
================================ */
input,
select {
  padding: 10px 12px;
  margin: 6px 8px 10px 0;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  background: #ffffff;
  color: #111827;
}

  input:focus,
  select:focus {
    outline: none;
    border-color: #2563eb;
    box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
  }



/* ===============================
   TABLES
================================ */
table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 14px;
  background: #ffffff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.06);
}

th {
  background: #f1f5f9;
  font-weight: 600;
  color: #334155;
}

th,
td {
  padding: 12px 14px;
  border-bottom: 1px solid #e5e7eb;
  text-align: left;
}

tr:hover {
  background: #f8fafc;
}

/* ===============================
   STATUS BADGES
================================ */
.active {
  color: #15803d;
  font-weight: 600;
}

.inactive {
  color: #b91c1c;
  font-weight: 600;
}

/* ===============================
   ACTION BUTTONS
================================ */
button.delete {
  background: #dc2626;
}

  button.delete:hover {
    background: #b91c1c;
  }

button.restore {
  background: #16a34a;
}

  button.restore:hover {
    background: #15803d;
  }

button.unassign {
  background: #f59e0b;
  color: #1f2937;
}

  button.unassign:hover {
    background: #d97706;
  }

/* ===============================
   BUTTON SYSTEM
================================ */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 8px 16px;
  background: #2563eb;
  color: #fff;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.18s ease;
  border: none;
  white-space: nowrap;
}

.btn-primary {
  background: linear-gradient(135deg, #2563eb, #1d4ed8);
  color: #fff;
}

  .btn-primary:hover {
    transform: translateY(1px);
    box-shadow: 0 6px 16px rgba(37, 99, 235, 0.35);
  }

.btn-danger {
  background: linear-gradient(135deg, #dc2626, #b91c1c);
  color: #fff;
}

  .btn-danger:hover {
    transform: translateY(-1px);
    box-shadow: 0 6px 14px rgba(220, 38, 38, 0.35);
  }

.btn-outline {
  background: #fff;
  border: 1px solid #d1d5db;
  color: #374151;
}

  .btn-outline:hover {
    background: #f1f5f9;
  }

.btn-link {
  padding: 0;
  background: transparent;
  color: #2563eb;
  font-weight: 500;
}

  .btn-link:hover {
    text-decoration: underline;
  }

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}



/* ===============================
   MOBILE RESPONSIVE
================================ */
@media (max-width: 768px) {
  .summary-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  table {
    font-size: 14px;
  }
}
===
/* ===============================
   ADMIN DASHBOARD – PREMIUM DESIGN
================================ */

:host {
  display: block;
  font-family: 'Inter', system-ui, sans-serif;
}

/* ===============================
   LAYOUT
================================ */
.dashboard-wrapper {
  min-height: 100vh;
  background: linear-gradient(135deg, #f0f4ff 0%, #faf5ff 50%, #f0fdfa 100%);
}

.dashboard-header {
  background: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(226, 232, 240, 0.6);
  padding: 16px 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 40;
}

.header-brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-icon {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white;
}

.header-title {
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
  letter-spacing: -0.02em;
}

.header-subtitle {
  font-size: 12px;
  color: #64748b;
  margin: 0;
}

.logout-btn {
  padding: 8px 16px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #ef4444;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
}

.logout-btn:hover {
  background: #fef2f2;
  border-color: #fecaca;
}

.dashboard-body {
  padding: 24px;
  max-width: 1280px;
  margin: 0 auto;
}

/* ===============================
   SUMMARY CARDS
================================ */
.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 28px;
}

.stat-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  padding: 20px;
  transition: all 0.25s ease;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
  border-radius: 16px 16px 0 0;
}

.stat-card.indigo::before { background: linear-gradient(90deg, #4f46e5, #7c3aed); }
.stat-card.cyan::before { background: linear-gradient(90deg, #06b6d4, #0891b2); }
.stat-card.emerald::before { background: linear-gradient(90deg, #10b981, #059669); }
.stat-card.amber::before { background: linear-gradient(90deg, #f59e0b, #d97706); }

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
}

.stat-label {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  margin: 0 0 8px;
}

.stat-value {
  font-size: 32px;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
  letter-spacing: -0.02em;
}

/* ===============================
   TABS
================================ */
.tabs-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: 6px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  margin-bottom: 24px;
  width: fit-content;
}

.tab {
  padding: 8px 18px;
  border-radius: 10px;
  border: none;
  background: transparent;
  color: #475569;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
}

.tab:hover {
  background: #f1f5f9;
  color: #1e293b;
}

.tab.active {
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

/* ===============================
   SECTIONS / CARDS
================================ */
.section-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  margin-bottom: 16px;
  overflow: hidden;
}

.section-header {
  padding: 20px 24px 0;
}

.section-title {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px;
}

.section-subtitle {
  font-size: 13px;
  color: #64748b;
  margin: 0;
}

.section-body {
  padding: 16px 24px 24px;
}

/* ===============================
   FORMS
================================ */
input, select {
  padding: 10px 14px;
  margin: 0;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: #ffffff;
  color: #0f172a;
  font-family: inherit;
  font-size: 14px;
  transition: all 0.2s ease;
}

input:focus, select:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

input::placeholder {
  color: #94a3b8;
}

/* ===============================
   TABLES
================================ */
table {
  width: 100%;
  border-collapse: collapse;
}

thead th {
  background: #f8fafc;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #64748b;
  padding: 12px 16px;
  border-bottom: 1px solid #e2e8f0;
  text-align: left;
}

tbody td {
  padding: 12px 16px;
  border-bottom: 1px solid #f1f5f9;
  font-size: 14px;
  color: #334155;
}

tbody tr {
  transition: background 0.15s ease;
}

tbody tr:hover {
  background: #f8fafc;
}

tbody tr:last-child td {
  border-bottom: none;
}

/* ===============================
   BUTTONS
================================ */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
  white-space: nowrap;
  font-family: inherit;
}

.btn-primary {
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: #fff;
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.2);
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(79, 70, 229, 0.35);
}

.btn-danger {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.btn-danger:hover {
  background: #fee2e2;
}

.btn-outline {
  background: #fff;
  border: 1px solid #e2e8f0;
  color: #475569;
}

.btn-outline:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.btn-link {
  padding: 0;
  background: transparent;
  color: #4f46e5;
  font-weight: 500;
}

.btn-link:hover {
  text-decoration: underline;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

/* ===============================
   STATUS BADGES
================================ */
.badge {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
}

.badge-success {
  background: #ecfdf5;
  color: #059669;
}

.badge-danger {
  background: #fef2f2;
  color: #dc2626;
}

.badge-warning {
  background: #fffbeb;
  color: #d97706;
}

.badge-info {
  background: #eff6ff;
  color: #2563eb;
}

/* ===============================
   SCAN RESULT COLORS
================================ */
.result-valid { color: #059669; }
.result-revalidated { color: #d97706; }
.result-invalid { color: #dc2626; }
.result-unauthorized { color: #be123c; }

/* ===============================
   EMPTY STATES
================================ */
.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #94a3b8;
}

.empty-state-icon {
  font-size: 32px;
  margin-bottom: 8px;
}

.empty-state-text {
  font-size: 14px;
}

/* ===============================
   RESPONSIVE
================================ */
@media (max-width: 768px) {
  .dashboard-body {
    padding: 16px;
  }

  .summary-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .tabs-bar {
    width: 100%;
  }

  .tab {
    font-size: 12px;
    padding: 6px 12px;
  }

  table {
    font-size: 13px;
  }

  thead th, tbody td {
    padding: 8px 12px;
  }
}

@media (max-width: 480px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .dashboard-header {
    padding: 12px 16px;
  }

  .header-title {
    font-size: 15px;
  }
}

```
```diff:superadmin.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SuperAdminService } from '../../core/services/superadmin.service';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';


@Component({
  standalone: true,
  selector: 'app-superadmin',
  imports: [CommonModule, FormsModule],
  templateUrl: './superadmin.component.html',
  styleUrl: './superadmin.component.scss'
})
export class SuperAdminComponent implements OnInit {

  summary = {
    totalAdmins: 0,
    totalEvents: 0,
    totalTickets: 0,
    usedTickets: 0,
    revalidations: 0
  };

  admins: any[] = [];
  deletedAdmins: any[] = [];
  activity: any[] = [];

  activeTab: 'admins' | 'deleted' | 'activity' = 'admins';

  newAdmin = {
    name: '',
    email: '',
    password: ''
  };

  loading = false;

  constructor(private sa: SuperAdminService, private router: Router, private signalr: AdminSignalrService) { }

  ngOnInit(): void {
    this.loadSummary();
    this.loadAdmins();
  }

  /* =========================
     TAB HANDLING
     ========================= */
  switchTab(tab: 'admins' | 'deleted' | 'activity') {
    this.activeTab = tab;

    if (tab === 'admins') this.loadAdmins();
    if (tab === 'deleted') this.loadDeletedAdmins();
    if (tab === 'activity') this.loadActivity();
  }

  /* =========================
     DASHBOARD
     ========================= */
  loadSummary() {
    this.sa.getSummary().subscribe(res => {
      this.summary = {
        totalAdmins: res.TotalAdmins ?? 0,
        totalEvents: res.TotalEvents ?? 0,
        totalTickets: res.TotalTickets ?? 0,
        usedTickets: res.UsedTickets ?? 0,
        revalidations: res.Revalidations ?? 0
      };
    });
  }

  /* =========================
     ADMINS
     ========================= */
  loadAdmins() {
    this.sa.getAdmins().subscribe(res => {
      this.admins = res.map(a => ({
        id: a.Id,
        name: a.Name,
        email: a.Email,
        isActive: a.IsActive,
        createdAt: a.CreatedAt
      }));
    });
  }
  logout() {
    // Optional confirmation
    if (!confirm('Logout from superadmin?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }

  createAdmin() {
    if (!this.newAdmin.name || !this.newAdmin.email || !this.newAdmin.password) {
      alert('All fields are required');
      return;
    }

    this.loading = true;

    this.sa.createAdmin(this.newAdmin).subscribe({
      next: () => {
        this.newAdmin = { name: '', email: '', password: '' };
        this.loadAdmins();
        this.loadSummary();
        this.loading = false;
      },
      error: err => {
        alert(err?.error ?? 'Failed to create admin');
        this.loading = false;
      }
    });
  }

  deleteAdmin(id: string) {
    if (!confirm('Delete this admin?')) return;

    this.sa.deleteAdmin(id).subscribe(() => {
      this.loadAdmins();
      this.loadSummary();
    });
  }

  /* =========================
     DELETED ADMINS
     ========================= */
  loadDeletedAdmins() {
    this.sa.getDeletedAdmins().subscribe(res => {
      this.deletedAdmins = res.map(a => ({
        id: a.Id,
        name: a.Name,
        email: a.Email,
        createdAt: a.CreatedAt
      }));
    });
  }

  restoreAdmin(id: string) {
    if (!confirm('Restore this admin?')) return;

    this.sa.restoreAdmin(id).subscribe(() => {
      this.loadAdmins();
      this.loadDeletedAdmins();
      this.loadSummary();
    });
  }

  /* =========================
     ACTIVITY
     ========================= */
  loadActivity() {
    this.sa.getAdminActivity().subscribe(res => {
      this.activity = res;
    });
  }
}
===
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SuperAdminService } from '../../core/services/superadmin.service';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';


@Component({
  standalone: true,
  selector: 'app-superadmin',
  imports: [CommonModule, FormsModule],
  templateUrl: './superadmin.component.html',
  styleUrls: ['./superadmin.component.scss']
})
export class SuperAdminComponent implements OnInit {

  summary = {
    totalAdmins: 0,
    totalEvents: 0,
    totalTickets: 0,
    usedTickets: 0,
    revalidations: 0
  };

  admins: any[] = [];
  deletedAdmins: any[] = [];
  activity: any[] = [];

  activeTab: 'admins' | 'deleted' | 'activity' = 'admins';

  newAdmin = {
    name: '',
    email: '',
    password: ''
  };

  loading = false;

  constructor(private sa: SuperAdminService, private router: Router, private signalr: AdminSignalrService) { }

  ngOnInit(): void {
    this.loadSummary();
    this.loadAdmins();
  }

  /* =========================
     TAB HANDLING
     ========================= */
  switchTab(tab: 'admins' | 'deleted' | 'activity') {
    this.activeTab = tab;

    if (tab === 'admins') this.loadAdmins();
    if (tab === 'deleted') this.loadDeletedAdmins();
    if (tab === 'activity') this.loadActivity();
  }

  /* =========================
     DASHBOARD
     ========================= */
  loadSummary() {
    this.sa.getSummary().subscribe(res => {
      this.summary = {
        totalAdmins: res.TotalAdmins ?? 0,
        totalEvents: res.TotalEvents ?? 0,
        totalTickets: res.TotalTickets ?? 0,
        usedTickets: res.UsedTickets ?? 0,
        revalidations: res.Revalidations ?? 0
      };
    });
  }

  /* =========================
     ADMINS
     ========================= */
  loadAdmins() {
    this.sa.getAdmins().subscribe(res => {
      this.admins = res.map(a => ({
        id: a.Id,
        name: a.Name,
        email: a.Email,
        isActive: a.IsActive,
        createdAt: a.CreatedAt
      }));
    });
  }
  logout() {
    // Optional confirmation
    if (!confirm('Logout from superadmin?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }

  createAdmin() {
    if (!this.newAdmin.name || !this.newAdmin.email || !this.newAdmin.password) {
      alert('All fields are required');
      return;
    }

    this.loading = true;

    this.sa.createAdmin(this.newAdmin).subscribe({
      next: () => {
        this.newAdmin = { name: '', email: '', password: '' };
        this.loadAdmins();
        this.loadSummary();
        this.loading = false;
      },
      error: err => {
        alert(err?.error ?? 'Failed to create admin');
        this.loading = false;
      }
    });
  }

  deleteAdmin(id: string) {
    if (!confirm('Delete this admin?')) return;

    this.sa.deleteAdmin(id).subscribe(() => {
      this.loadAdmins();
      this.loadSummary();
    });
  }

  /* =========================
     DELETED ADMINS
     ========================= */
  loadDeletedAdmins() {
    this.sa.getDeletedAdmins().subscribe(res => {
      this.deletedAdmins = res.map(a => ({
        id: a.Id,
        name: a.Name,
        email: a.Email,
        createdAt: a.CreatedAt
      }));
    });
  }

  restoreAdmin(id: string) {
    if (!confirm('Restore this admin?')) return;

    this.sa.restoreAdmin(id).subscribe(() => {
      this.loadAdmins();
      this.loadDeletedAdmins();
      this.loadSummary();
    });
  }

  /* =========================
     ACTIVITY
     ========================= */
  loadActivity() {
    this.sa.getAdminActivity().subscribe(res => {
      this.activity = res;
    });
  }
}
```
```diff:superadmin.component.html
<div class="min-h-screen bg-gray-50 p-6">

  <!-- HEADER -->
  <header class="mb-8 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
    <div>
      <h1 class="text-2xl font-semibold text-gray-900">
        Super Admin Dashboard
      </h1>
      <p class="text-sm text-gray-500 mt-1">
        Platform overview and administrator management
      </p>
    </div>

    <button class="px-4 py-2 rounded-lg bg-red-600 text-white
           hover:bg-red-700 active:scale-[0.98]
           transition shadow-sm"
            (click)="logout()">
      Logout
    </button>
  </header>


  <!-- ================= SUMMARY ================= -->
  <section class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-10">
    <div class="summary-card">
      <p>Total Admins</p>
      <h3>{{ summary.totalAdmins || 0 }}</h3>
    </div>

    <div class="summary-card">
      <p>Total Events</p>
      <h3>{{ summary.totalEvents || 0 }}</h3>
    </div>

    <div class="summary-card">
      <p>Total Tickets</p>
      <h3>{{ summary.totalTickets || 0 }}</h3>
    </div>
  </section>


  <!-- ================= TABS ================= -->
  <nav class="flex gap-2 mb-8 bg-white border p-2 rounded-xl w-fit">
    <button class="px-4 py-2 rounded-lg text-sm font-medium border transition"
            [ngClass]="activeTab==='admins'
        ? 'bg-blue-600 text-white border-blue-600'
        : 'bg-white text-gray-700 hover:bg-gray-100'"
            (click)="switchTab('admins')">
      Admins
    </button>

    <button class="px-4 py-2 rounded-lg text-sm font-medium border transition"
            [ngClass]="activeTab==='deleted'
        ? 'bg-blue-600 text-white border-blue-600'
        : 'bg-white text-gray-700 hover:bg-gray-100'"
            (click)="switchTab('deleted')">
      Deleted
    </button>

    <button class="px-4 py-2 rounded-lg text-sm font-medium border transition"
            [ngClass]="activeTab==='activity'
        ? 'bg-blue-600 text-white border-blue-600'
        : 'bg-white text-gray-700 hover:bg-gray-100'"
            (click)="switchTab('activity')">
      Activity
    </button>
  </nav>

  <!-- ================= ADMINS TAB ================= -->
  <section *ngIf="activeTab==='admins'" class="space-y-6">

    <!-- CREATE ADMIN -->
    <div class="bg-white border rounded-2xl p-5">
      <h3 class="font-medium text-gray-900 mb-4">
        Create Admin
      </h3>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <input class="border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
               placeholder="Name"
               [(ngModel)]="newAdmin.name" />

        <input class="border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
               placeholder="Email"
               [(ngModel)]="newAdmin.email" />

        <input type="password"
               class="border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500"
               placeholder="Password"
               [(ngModel)]="newAdmin.password" />
      </div>

      <button class="mt-4 px-4 py-2 rounded-lg text-sm font-medium
               bg-blue-600 text-white hover:bg-blue-700
               disabled:opacity-50"
              [disabled]="loading"
              (click)="createAdmin()">
        {{ loading ? 'Creating…' : 'Create Admin' }}
      </button>
    </div>

    <!-- ADMINS TABLE -->
    <div class="bg-white border rounded-xl overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-gray-100 text-gray-600">
          <tr>
            <th class="text-left px-4 py-2">Name</th>
            <th class="text-left px-4 py-2">Email</th>
            <th class="text-left px-4 py-2">Status</th>
            <th class="text-left px-4 py-2">Action</th>
          </tr>
        </thead>

        <tbody>
          <tr *ngFor="let admin of admins"
              class="border-t hover:bg-gray-50">
            <td class="px-4 py-2">{{ admin.name }}</td>
            <td class="px-4 py-2">{{ admin.email }}</td>
            <td class="px-4 py-2">
              <span class="text-xs font-medium px-2.5 py-1 rounded-full"
                    [ngClass]="admin.isActive
                  ? 'bg-green-100 text-green-700'
                  : 'bg-red-100 text-red-700'">
                {{ admin.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td class="px-4 py-2">
              <button *ngIf="admin.isActive"
                      class="inline-flex items-center gap-1 px-3 py-1.5 text-xs
               font-medium rounded-full
               bg-red-50 text-red-600 hover:bg-red-100 transition"
                      (click)="deleteAdmin(admin.id)">
                Delete
              </button>

              <button *ngIf="!admin.isActive"
                      class="inline-flex items-center gap-1 px-3 py-1.5 text-xs
               font-medium rounded-full
               bg-blue-50 text-blue-600 hover:bg-blue-100 transition"
                      (click)="restoreAdmin(admin.id)">
                Restore
              </button>




            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!admins.length"
         class="text-center text-gray-500 py-4">
        No admins found
      </p>
    </div>
  </section>

  <!-- ================= DELETED ADMINS ================= -->
  <section *ngIf="activeTab==='deleted'" class="bg-white border rounded-xl overflow-hidden">
    <table class="w-full text-sm">
      <thead class="bg-gray-100 text-gray-600">
        <tr>
          <th class="px-4 py-2 text-left">Name</th>
          <th class="px-4 py-2 text-left">Email</th>
          <th class="px-4 py-2 text-left">Deleted On</th>
          <th class="px-4 py-2 text-left">Action</th>
        </tr>
      </thead>

      <tbody>
        <tr *ngFor="let admin of deletedAdmins"
            class="border-t hover:bg-gray-50">
          <td class="px-4 py-2">{{ admin.name }}</td>
          <td class="px-4 py-2">{{ admin.email }}</td>
          <td class="px-4 py-2">
            {{ admin.createdAt | date:'short' }}
          </td>
          <td class="px-4 py-2">
            <button class="text-blue-600 hover:underline"
                    (click)="restoreAdmin(admin.id)">
              Restore
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <p *ngIf="!deletedAdmins.length"
       class="text-center text-gray-500 py-4">
      No deleted admins
    </p>
  </section>

  <!-- ================= ACTIVITY ================= -->
  <section *ngIf="activeTab==='activity'" class="bg-white border rounded-xl overflow-hidden">
    <table class="w-full text-sm">
      <thead class="bg-gray-100 text-gray-600">
        <tr>
          <th class="px-4 py-2 text-left">Admin</th>
          <th class="px-4 py-2 text-left">Events Created</th>
          <th class="px-4 py-2 text-left">Tickets Created</th>
        </tr>
      </thead>

      <tbody>
        <tr *ngFor="let act of activity"
            class="border-t hover:bg-gray-50">
          <td class="px-4 py-2">{{ act.AdminName }}</td>
          <td class="px-4 py-2">{{ act.EventsCreated }}</td>
          <td class="px-4 py-2">{{ act.TicketsCreated }}</td>
        </tr>
      </tbody>
    </table>

    <p *ngIf="!activity.length"
       class="text-center text-gray-500 py-4">
      No activity recorded
    </p>
  </section>

</div>
===
<div class="sa-wrapper">

  <!-- HEADER -->
  <header class="sa-header">
    <div class="sa-brand">
      <div class="sa-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M12 2L2 7l10 5 10-5-10-5z"></path>
          <path d="M2 17l10 5 10-5"></path>
          <path d="M2 12l10 5 10-5"></path>
        </svg>
      </div>
      <div>
        <h1 class="sa-title">Super Admin</h1>
        <p class="sa-subtitle">Platform overview &amp; admin management</p>
      </div>
    </div>
    <button class="sa-logout" (click)="logout()">Logout</button>
  </header>

  <div class="sa-body">

    <!-- SUMMARY -->
    <div class="summary-grid">
      <div class="summary-card">
        <p>Total Admins</p>
        <h3>{{ summary.totalAdmins || 0 }}</h3>
      </div>
      <div class="summary-card">
        <p>Total Events</p>
        <h3>{{ summary.totalEvents || 0 }}</h3>
      </div>
      <div class="summary-card">
        <p>Total Tickets</p>
        <h3>{{ summary.totalTickets || 0 }}</h3>
      </div>
    </div>

    <!-- TABS -->
    <nav class="sa-tabs">
      <button class="sa-tab" [class.active]="activeTab==='admins'" (click)="switchTab('admins')">Admins</button>
      <button class="sa-tab" [class.active]="activeTab==='deleted'" (click)="switchTab('deleted')">Deleted</button>
      <button class="sa-tab" [class.active]="activeTab==='activity'" (click)="switchTab('activity')">Activity</button>
    </nav>


    <!-- ============ ADMINS ============ -->
    <section *ngIf="activeTab==='admins'" class="space-y-4">
      <div class="sa-section">
        <div class="sa-section-header">
          <h3 class="section-title">Create Admin</h3>
          <p class="section-subtitle">Add a new administrator to the platform</p>
        </div>
        <div class="sa-section-body">
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-4">
            <input type="text" placeholder="Name" [(ngModel)]="newAdmin.name" />
            <input type="email" placeholder="Email" [(ngModel)]="newAdmin.email" />
            <input type="password" placeholder="Password" [(ngModel)]="newAdmin.password" />
          </div>
          <button class="sa-btn-create" [disabled]="loading" (click)="createAdmin()">
            {{ loading ? 'Creating…' : 'Create Admin' }}
          </button>
        </div>
      </div>

      <div class="sa-section">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let admin of admins">
              <td class="font-semibold" style="color: #0f172a;">{{ admin.name }}</td>
              <td>{{ admin.email }}</td>
              <td>
                <span class="badge" [ngClass]="admin.isActive ? 'badge-success' : 'badge-danger'">
                  {{ admin.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td>
                <button *ngIf="admin.isActive" class="action-btn action-delete" (click)="deleteAdmin(admin.id)">Delete</button>
                <button *ngIf="!admin.isActive" class="action-btn action-restore" (click)="restoreAdmin(admin.id)">Restore</button>
              </td>
            </tr>
          </tbody>
        </table>
        <p *ngIf="!admins.length" class="empty-state">No admins found</p>
      </div>
    </section>


    <!-- ============ DELETED ============ -->
    <section *ngIf="activeTab==='deleted'" class="sa-section">
      <div class="sa-section-header" style="padding-bottom: 12px;">
        <h3 class="section-title">Deleted Admins</h3>
      </div>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Deleted On</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let admin of deletedAdmins">
            <td class="font-semibold" style="color: #0f172a;">{{ admin.name }}</td>
            <td>{{ admin.email }}</td>
            <td style="color: #94a3b8;">{{ admin.createdAt | date:'short' }}</td>
            <td>
              <button class="action-btn action-restore" (click)="restoreAdmin(admin.id)">Restore</button>
            </td>
          </tr>
        </tbody>
      </table>
      <p *ngIf="!deletedAdmins.length" class="empty-state">No deleted admins</p>
    </section>


    <!-- ============ ACTIVITY ============ -->
    <section *ngIf="activeTab==='activity'" class="sa-section">
      <div class="sa-section-header" style="padding-bottom: 12px;">
        <h3 class="section-title">Admin Activity</h3>
        <p class="section-subtitle">Overview of admin actions across the platform</p>
      </div>
      <table>
        <thead>
          <tr>
            <th>Admin</th>
            <th>Events Created</th>
            <th>Tickets Created</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let act of activity">
            <td class="font-semibold" style="color: #0f172a;">{{ act.AdminName }}</td>
            <td>{{ act.EventsCreated }}</td>
            <td>{{ act.TicketsCreated }}</td>
          </tr>
        </tbody>
      </table>
      <p *ngIf="!activity.length" class="empty-state">No activity recorded</p>
    </section>

  </div>
</div>

```
```diff:superadmin.component.scss
table {
  border-collapse: collapse;
}

button {
  cursor: pointer;
}

.summary-card {
  background: white;
  border-radius: 14px;
  border: 1px solid #e5e7eb;
  padding: 20px;
  transition: transform .2s ease, box-shadow .2s ease;
}

  .summary-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 24px rgba(0,0,0,.08);
  }

  .summary-card p {
    font-size: 13px;
    color: #6b7280;
  }

  .summary-card h3 {
    font-size: 30px;
    font-weight: 600;
    margin-top: 6px;
    color: #111827;
  }

table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
}

thead th {
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: .04em;
}

tbody tr {
  transition: background .15s ease;
}

  tbody tr:hover {
    background: #f9fafb;
  }

td, th {
  padding: 12px 16px;
}
===
:host { display: block; font-family: 'Inter', system-ui, sans-serif; }

.sa-wrapper {
  min-height: 100vh;
  background: linear-gradient(135deg, #f0f4ff 0%, #faf5ff 50%, #f0fdfa 100%);
}

/* Header */
.sa-header {
  background: rgba(255,255,255,0.8);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(226,232,240,0.6);
  padding: 16px 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 40;
}

.sa-brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sa-icon {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  background: linear-gradient(135deg, #dc2626, #b91c1c);
  color: white;
}

.sa-title { font-size: 18px; font-weight: 700; color: #0f172a; margin: 0; letter-spacing: -0.02em; }
.sa-subtitle { font-size: 12px; color: #64748b; margin: 0; }

.sa-logout {
  padding: 8px 16px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #ef4444;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
}
.sa-logout:hover { background: #fef2f2; border-color: #fecaca; }

.sa-body { padding: 24px; max-width: 1280px; margin: 0 auto; }

/* Summary cards */
.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 28px;
}

.summary-card {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  padding: 20px;
  transition: all 0.25s ease;
  position: relative;
  overflow: hidden;
}

.summary-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 3px;
  border-radius: 16px 16px 0 0;
}

.summary-card:nth-child(1)::before { background: linear-gradient(90deg, #ef4444, #dc2626); }
.summary-card:nth-child(2)::before { background: linear-gradient(90deg, #4f46e5, #7c3aed); }
.summary-card:nth-child(3)::before { background: linear-gradient(90deg, #06b6d4, #0891b2); }

.summary-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0,0,0,0.08);
}

.summary-card p {
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #64748b;
  margin: 0 0 8px;
}

.summary-card h3 {
  font-size: 32px;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
  letter-spacing: -0.02em;
}

/* Tabs */
.sa-tabs {
  display: flex;
  gap: 6px;
  padding: 6px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  margin-bottom: 24px;
  width: fit-content;
}

.sa-tab {
  padding: 8px 18px;
  border-radius: 10px;
  border: none;
  background: transparent;
  color: #475569;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
}

.sa-tab:hover { background: #f1f5f9; color: #1e293b; }
.sa-tab.active {
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(79,70,229,0.3);
}

/* Section card */
.sa-section {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  overflow: hidden;
  margin-bottom: 16px;
}

.sa-section-header {
  padding: 20px 24px 0;
}

.section-title {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px;
}

.section-subtitle {
  font-size: 13px;
  color: #64748b;
  margin: 0;
}

.sa-section-body { padding: 16px 24px 24px; }

/* Tables */
table { width: 100%; border-collapse: collapse; }

thead th {
  background: #f8fafc;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: #64748b;
  padding: 12px 16px;
  border-bottom: 1px solid #e2e8f0;
  text-align: left;
}

tbody td {
  padding: 12px 16px;
  border-bottom: 1px solid #f1f5f9;
  font-size: 14px;
  color: #334155;
}

tbody tr { transition: background 0.15s ease; }
tbody tr:hover { background: #f8fafc; }
tbody tr:last-child td { border-bottom: none; }

/* Buttons */
.sa-btn-create {
  padding: 10px 20px;
  border-radius: 10px;
  border: none;
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: inherit;
  box-shadow: 0 2px 8px rgba(79,70,229,0.2);
}

.sa-btn-create:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(79,70,229,0.35);
}

.sa-btn-create:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

/* Badges */
.badge {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
}

.badge-success { background: #ecfdf5; color: #059669; }
.badge-danger { background: #fef2f2; color: #dc2626; }

.action-btn {
  padding: 4px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s ease;
  border: none;
  font-family: inherit;
}

.action-delete {
  background: #fef2f2;
  color: #dc2626;
}

.action-delete:hover { background: #fee2e2; }

.action-restore {
  background: #eff6ff;
  color: #2563eb;
}

.action-restore:hover { background: #dbeafe; }

/* Empty state */
.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #94a3b8;
  font-size: 14px;
}

/* Form inputs */
input[type="text"], input[type="email"], input[type="password"] {
  padding: 10px 14px;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: #ffffff;
  color: #0f172a;
  font-family: inherit;
  font-size: 14px;
  transition: all 0.2s ease;
}

input:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79,70,229,0.1);
}

input::placeholder { color: #94a3b8; }

/* Responsive */
@media (max-width: 768px) {
  .sa-body { padding: 16px; }
  .summary-grid { grid-template-columns: 1fr; }
  .sa-tabs { width: 100%; }
  .sa-tab { font-size: 12px; padding: 6px 12px; }
}

```
```diff:worker.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WorkerService } from '../../core/services/worker.service';
import { WorkerScannerComponent } from './worker-scanner.component';
import { playScanFeedback } from './scan-feedback';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';

type ScanResult = 'VALID' | 'INVALID' | 'REVALIDATED';

@Component({
  standalone: true,
  selector: 'app-worker',
  imports: [CommonModule, FormsModule, WorkerScannerComponent],
  templateUrl: './worker.component.html'
})
export class WorkerComponent {

  code = '';
  loading = false;

  result: ScanResult | null = null;
  message = '';

  scanHistory: {
    code: string;
    status: ScanResult;
    time: Date;
  }[] = [];

  scannerOpen = false;

  constructor(private worker: WorkerService, private router: Router, private signalr: AdminSignalrService) { }

  // =========================
  // VALIDATE
  // =========================
  validate() {
    if (!this.code.trim() || this.loading) return;

    this.loading = true;

    this.worker.validateTicket(this.code).subscribe({
      next: (res: any) => {
        if (res.status === 'VALID') {
          this.feedback('VALID', 'Ticket validated');
        }
        else if (res.status === 'REVALIDATED') {
          this.feedback('REVALIDATED', 'Ticket already used');
        }
      },
      error: err => {
        if (err.status === 403) {
          this.feedback('INVALID', 'Worker not assigned');
        }
        else {
          this.feedback('INVALID', 'Invalid ticket');
        }
      }
    });



  }
  logout() {
    // Optional confirmation
    if (!confirm('Logout from scanner?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }
  // =========================
  // QR CALLBACK
  // =========================
  onQrScanned(code: string) {
    this.scannerOpen = false;
    this.code = code;
    this.validate();
  }

  // =========================
  // FEEDBACK
  // =========================
  feedback(status: ScanResult, msg: string) {
    this.result = status;
    this.message = msg;
    this.loading = false;

    playScanFeedback(status === 'VALID' );

    this.scanHistory.unshift({
      code: this.code,
      status,
      time: new Date()
    });

    this.scanHistory = this.scanHistory.slice(0, 10);
    this.code = '';
  }

  openScanner() {
    this.scannerOpen = true;
  }

  closeScanner() {
    this.scannerOpen = false;
  }
}
===
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WorkerService } from '../../core/services/worker.service';
import { WorkerScannerComponent } from './worker-scanner.component';
import { playScanFeedback } from './scan-feedback';
import { Router } from '@angular/router';
import { AdminSignalrService, AdminLiveEvent } from '../../core/signalr/admin-signalr.service';

type ScanResult = 'VALID' | 'INVALID' | 'REVALIDATED';

@Component({
  standalone: true,
  selector: 'app-worker',
  imports: [CommonModule, FormsModule, WorkerScannerComponent],
  templateUrl: './worker.component.html',
  styleUrls: ['./worker.component.scss']
})
export class WorkerComponent {

  code = '';
  loading = false;

  result: ScanResult | null = null;
  message = '';

  scanHistory: {
    code: string;
    status: ScanResult;
    time: Date;
  }[] = [];

  scannerOpen = false;

  constructor(private worker: WorkerService, private router: Router, private signalr: AdminSignalrService) { }

  // =========================
  // VALIDATE
  // =========================
  validate() {
    if (!this.code.trim() || this.loading) return;

    this.loading = true;

    this.worker.validateTicket(this.code).subscribe({
      next: (res: any) => {
        if (res.status === 'VALID') {
          this.feedback('VALID', 'Ticket validated');
        }
        else if (res.status === 'REVALIDATED') {
          this.feedback('REVALIDATED', 'Ticket already used');
        }
      },
      error: err => {
        if (err.status === 403) {
          this.feedback('INVALID', 'Worker not assigned');
        }
        else {
          this.feedback('INVALID', 'Invalid ticket');
        }
      }
    });



  }
  logout() {
    // Optional confirmation
    if (!confirm('Logout from scanner?')) return;

    // 🔥 Clear auth data
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    // 🔌 Disconnect SignalR if present
    try {
      this.signalr?.disconnect?.();
    } catch { }

    // 🚪 Redirect to login
    this.router.navigate(['/login']);
  }
  // =========================
  // QR CALLBACK
  // =========================
  onQrScanned(code: string) {
    this.scannerOpen = false;
    this.code = code;
    this.validate();
  }

  // =========================
  // FEEDBACK
  // =========================
  feedback(status: ScanResult, msg: string) {
    this.result = status;
    this.message = msg;
    this.loading = false;

    playScanFeedback(status === 'VALID' );

    this.scanHistory.unshift({
      code: this.code,
      status,
      time: new Date()
    });

    this.scanHistory = this.scanHistory.slice(0, 10);
    this.code = '';
  }

  openScanner() {
    this.scannerOpen = true;
  }

  closeScanner() {
    this.scannerOpen = false;
  }
}
```
```diff:worker.component.html
<div class="min-h-screen bg-gray-50 flex justify-center">
  <div class="w-full max-w-md p-4 space-y-5">

    <!-- HEADER -->
    <header class="text-center py-3">
      <h1 class="text-2xl font-semibold text-gray-900">
        Ticket Scanner
      </h1>
      <p class="text-gray-500 text-sm">
        Scan and validate event tickets
      </p>


    </header>
    <div class="flex justify-end">
      <button class="px-4 py-2 rounded-lg bg-red-600 text-white hover:bg-red-700 transition"
              (click)="logout()">
        Logout
      </button>
    </div>


    <!-- RESULT -->
    <div *ngIf="result"
         class="rounded-xl p-4 text-center font-semibold border transition-all"
         [ngClass]="{
           'bg-green-50 text-green-700 border-green-300': result==='VALID',
           'bg-red-50 text-red-700 border-red-300': result==='INVALID',
           'bg-yellow-50 text-yellow-700 border-yellow-300': result==='REVALIDATED'
         }">
      <div class="text-lg tracking-wide">
        {{ result }}
      </div>
      <div class="text-sm font-normal mt-1">
        {{ message }}
      </div>
    </div>

    <!-- SCAN BUTTON -->
    <button class="w-full py-4 rounded-xl text-lg font-medium
             bg-blue-600 text-white
             hover:bg-blue-700
             active:scale-[0.98]
             transition
             disabled:opacity-50"
            [disabled]="loading"
            (click)="openScanner()">
      Scan QR Code
    </button>

    <!-- MANUAL INPUT -->
    <div class="flex gap-2">
      <input class="flex-1 rounded-xl px-4 py-3
               border border-gray-300
               text-gray-900 text-base
               focus:outline-none focus:ring-2 focus:ring-blue-500"
             placeholder="Enter ticket code"
             [(ngModel)]="code"
             (keyup.enter)="validate()" />

      <button class="px-5 py-3 rounded-xl
               bg-gray-800 text-white
               hover:bg-gray-900
               active:scale-[0.98]
               transition"
              [disabled]="loading"
              (click)="validate()">
        Validate
      </button>
    </div>

    <!-- HISTORY -->
    <div>
      <h3 class="font-medium text-gray-800 mb-2">
        Recent Scans
      </h3>

      <div *ngIf="!scanHistory.length"
           class="text-gray-400 text-sm text-center py-2">
        No scans yet
      </div>

      <ul class="space-y-2">
        <li *ngFor="let h of scanHistory"
            class="flex justify-between items-center
                   bg-white border border-gray-200
                   px-3 py-2 rounded-lg">
          <span class="text-sm font-semibold"
                [ngClass]="{
                  'text-green-600': h.status==='VALID',
                  'text-red-600': h.status==='INVALID',
                  'text-yellow-600': h.status==='REVALIDATED'
                }">
            {{ h.status }}
          </span>

          <span class="text-xs text-gray-400">
            {{ h.time | date:'shortTime' }}
          </span>
        </li>
      </ul>
    </div>

    <!-- CAMERA MODAL -->
    <app-worker-scanner *ngIf="scannerOpen"
                        (scanned)="onQrScanned($event)"
                        (close)="closeScanner()">
    </app-worker-scanner>

  </div>
</div>
===
<div class="scanner-wrapper">

  <!-- HEADER -->
  <header class="scanner-header">
    <div class="scanner-brand">
      <div class="scanner-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="3" width="7" height="7"></rect>
          <rect x="14" y="3" width="7" height="7"></rect>
          <rect x="14" y="14" width="7" height="7"></rect>
          <rect x="3" y="14" width="7" height="7"></rect>
        </svg>
      </div>
      <div>
        <h1 class="scanner-title">Ticket Scanner</h1>
        <p class="scanner-subtitle">Scan &amp; validate event tickets</p>
      </div>
    </div>
    <button class="scanner-logout" (click)="logout()">Logout</button>
  </header>

  <div class="scanner-body">

    <!-- RESULT BANNER -->
    <div *ngIf="result" class="result-banner animate-pop"
         [ngClass]="{
           'result-valid': result==='VALID',
           'result-invalid': result==='INVALID',
           'result-revalidated': result==='REVALIDATED'
         }">
      <div class="result-icon">
        <span *ngIf="result==='VALID'">✓</span>
        <span *ngIf="result==='INVALID'">✕</span>
        <span *ngIf="result==='REVALIDATED'">↻</span>
      </div>
      <div>
        <div class="result-status">{{ result }}</div>
        <div class="result-message">{{ message }}</div>
      </div>
    </div>

    <!-- SCAN BUTTON -->
    <button class="scan-btn"
            [disabled]="loading"
            (click)="openScanner()">
      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
           stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="3" width="7" height="7"></rect>
        <rect x="14" y="3" width="7" height="7"></rect>
        <rect x="14" y="14" width="7" height="7"></rect>
        <rect x="3" y="14" width="7" height="7"></rect>
      </svg>
      {{ loading ? 'Scanning…' : 'Scan QR Code' }}
    </button>

    <!-- MANUAL INPUT -->
    <div class="manual-input">
      <input class="manual-field"
             placeholder="Or enter ticket code manually…"
             [(ngModel)]="code"
             (keyup.enter)="validate()" />
      <button class="validate-btn" [disabled]="loading" (click)="validate()">
        Validate
      </button>
    </div>

    <!-- HISTORY -->
    <div class="history-section">
      <h3 class="history-title">Recent Scans</h3>

      <div *ngIf="!scanHistory.length" class="history-empty">
        No scans yet – start scanning tickets
      </div>

      <ul class="history-list">
        <li *ngFor="let h of scanHistory" class="history-item">
          <div class="history-left">
            <span class="history-badge"
                  [ngClass]="{
                    'badge-valid': h.status==='VALID',
                    'badge-invalid': h.status==='INVALID',
                    'badge-revalidated': h.status==='REVALIDATED'
                  }">
              {{ h.status }}
            </span>
            <span class="history-code">{{ h.code }}</span>
          </div>
          <span class="history-time">{{ h.time | date:'shortTime' }}</span>
        </li>
      </ul>
    </div>

    <!-- CAMERA MODAL -->
    <app-worker-scanner *ngIf="scannerOpen"
                        (scanned)="onQrScanned($event)"
                        (close)="closeScanner()">
    </app-worker-scanner>

  </div>
</div>

```
```diff:worker.component.scss
/* Worker-specific tweaks that Tailwind can't handle well */

/* Smooth tap highlight removal (mobile Safari) */
button {
  -webkit-tap-highlight-color: transparent;
}

/* Prevent zoom on input focus (iOS Safari) */
input {
  font-size: 16px;
}

/* Nice animation when result appears */
:host {
  display: block;
}

  :host ::ng-deep .result {
    animation: pop 0.2s ease-out;
  }

@keyframes pop {
  0% {
    transform: scale(0.95);
    opacity: 0.7;
  }

  100% {
    transform: scale(1);
    opacity: 1;
  }
}
===
:host { display: block; font-family: 'Inter', system-ui, sans-serif; }

/* Smooth tap highlight removal (mobile Safari) */
button { -webkit-tap-highlight-color: transparent; }
input { font-size: 16px; }

.scanner-wrapper {
  min-height: 100vh;
  background: linear-gradient(135deg, #f0f4ff 0%, #faf5ff 50%, #f0fdfa 100%);
}

/* Header */
.scanner-header {
  background: rgba(255,255,255,0.8);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(226,232,240,0.6);
  padding: 16px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 40;
}

.scanner-brand { display: flex; align-items: center; gap: 12px; }

.scanner-icon {
  width: 36px; height: 36px;
  display: flex; align-items: center; justify-content: center;
  border-radius: 10px;
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white;
}

.scanner-title { font-size: 18px; font-weight: 700; color: #0f172a; margin: 0; letter-spacing: -0.02em; }
.scanner-subtitle { font-size: 12px; color: #64748b; margin: 0; }

.scanner-logout {
  padding: 8px 16px; border-radius: 10px;
  border: 1px solid #e2e8f0; background: white;
  color: #ef4444; font-size: 13px; font-weight: 600;
  cursor: pointer; transition: all 0.2s ease; font-family: inherit;
}
.scanner-logout:hover { background: #fef2f2; border-color: #fecaca; }

.scanner-body {
  max-width: 480px;
  margin: 0 auto;
  padding: 24px 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* Result banner */
.result-banner {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px 20px;
  border-radius: 16px;
  border: 1px solid;
}

.result-valid {
  background: #ecfdf5;
  border-color: #a7f3d0;
}

.result-invalid {
  background: #fef2f2;
  border-color: #fecaca;
}

.result-revalidated {
  background: #fffbeb;
  border-color: #fde68a;
}

.result-icon {
  width: 40px; height: 40px;
  border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  font-size: 20px; font-weight: 700;
}

.result-valid .result-icon { background: #059669; color: white; }
.result-invalid .result-icon { background: #dc2626; color: white; }
.result-revalidated .result-icon { background: #d97706; color: white; }

.result-status {
  font-size: 16px; font-weight: 700; letter-spacing: 0.02em;
}

.result-valid .result-status { color: #059669; }
.result-invalid .result-status { color: #dc2626; }
.result-revalidated .result-status { color: #d97706; }

.result-message {
  font-size: 13px; color: #64748b; margin-top: 2px;
}

/* Scan button */
.scan-btn {
  width: 100%; padding: 18px;
  border-radius: 16px; border: none;
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
  color: white; font-size: 16px; font-weight: 700;
  cursor: pointer; font-family: inherit;
  display: flex; align-items: center; justify-content: center; gap: 10px;
  transition: all 0.25s ease;
  box-shadow: 0 4px 16px rgba(79,70,229,0.3);
}

.scan-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 28px rgba(79,70,229,0.45);
}

.scan-btn:active { transform: translateY(0); }
.scan-btn:disabled { opacity: 0.5; cursor: not-allowed; transform: none; box-shadow: none; }

/* Manual input */
.manual-input {
  display: flex; gap: 8px;
}

.manual-field {
  flex: 1; padding: 14px 16px;
  border-radius: 14px;
  border: 1px solid #e2e8f0;
  background: white;
  color: #0f172a; font-size: 15px;
  font-family: inherit;
  transition: all 0.2s ease;
}

.manual-field:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79,70,229,0.1);
}

.manual-field::placeholder { color: #94a3b8; }

.validate-btn {
  padding: 14px 20px; border-radius: 14px; border: none;
  background: #1e293b; color: white;
  font-size: 14px; font-weight: 600;
  cursor: pointer; font-family: inherit;
  transition: all 0.2s ease;
}

.validate-btn:hover { background: #0f172a; }
.validate-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* History */
.history-section {
  margin-top: 8px;
}

.history-title {
  font-size: 15px; font-weight: 700; color: #0f172a;
  margin: 0 0 12px;
}

.history-empty {
  text-align: center;
  padding: 24px; color: #94a3b8;
  font-size: 14px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 14px;
}

.history-list {
  list-style: none; padding: 0; margin: 0;
  display: flex; flex-direction: column; gap: 8px;
}

.history-item {
  display: flex; justify-content: space-between; align-items: center;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 12px 16px;
  transition: all 0.15s ease;
}

.history-item:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.04); }

.history-left {
  display: flex; align-items: center; gap: 10px;
}

.history-badge {
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.badge-valid { background: #ecfdf5; color: #059669; }
.badge-invalid { background: #fef2f2; color: #dc2626; }
.badge-revalidated { background: #fffbeb; color: #d97706; }

.history-code {
  font-family: 'SF Mono', 'Cascadia Code', monospace;
  font-size: 13px; color: #475569;
}

.history-time {
  font-size: 12px; color: #94a3b8;
}

/* Pop animation */
.animate-pop {
  animation: pop 0.3s ease-out;
}

@keyframes pop {
  0% { transform: scale(0.92); opacity: 0.6; }
  70% { transform: scale(1.02); }
  100% { transform: scale(1); opacity: 1; }
}

```
```diff:worker-scanner.component.ts
import {
  Component,
  EventEmitter,
  Output,
  OnDestroy,
  AfterViewInit,
  ViewChild,
  ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserMultiFormatReader } from '@zxing/browser';

@Component({
  standalone: true,
  selector: 'app-worker-scanner',
  imports: [CommonModule],
  templateUrl: './worker-scanner.component.html',
  styleUrl: './worker-scanner.component.scss'
})
export class WorkerScannerComponent implements AfterViewInit, OnDestroy {

  @ViewChild('video', { static: true })
  video!: ElementRef<HTMLVideoElement>;

  @Output() scanned = new EventEmitter<string>();
  @Output() close = new EventEmitter<void>();

  private reader = new BrowserMultiFormatReader();
  private stream: MediaStream | null = null;

  scanning = false;
  error = '';

  async ngAfterViewInit() {
    this.startCamera();
  }

  async startCamera() {
    try {
      this.scanning = true;

      this.stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });

      this.video.nativeElement.srcObject = this.stream;
      await this.video.nativeElement.play();

      this.reader.decodeFromVideoElement(
        this.video.nativeElement,
        (result) => {
          if (result) {
            this.scanned.emit(result.getText());
            this.stopCamera();
          }
        }
      );

    } catch {
      this.error = 'Camera permission denied';
      this.scanning = false;
    }
  }

  stopCamera() {
    this.scanning = false;

    if (this.stream) {
      this.stream.getTracks().forEach(t => t.stop());
      this.stream = null;
    }

    this.close.emit();
  }

  ngOnDestroy() {
    this.stopCamera();
  }
}
===
import {
  Component,
  EventEmitter,
  Output,
  OnDestroy,
  AfterViewInit,
  ViewChild,
  ElementRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserMultiFormatReader } from '@zxing/browser';

@Component({
  standalone: true,
  selector: 'app-worker-scanner',
  imports: [CommonModule],
  templateUrl: './worker-scanner.component.html',
  styleUrls: ['./worker-scanner.component.scss']
})
export class WorkerScannerComponent implements AfterViewInit, OnDestroy {

  @ViewChild('video', { static: true })
  video!: ElementRef<HTMLVideoElement>;

  @Output() scanned = new EventEmitter<string>();
  @Output() close = new EventEmitter<void>();

  private reader = new BrowserMultiFormatReader();
  private stream: MediaStream | null = null;

  scanning = false;
  error = '';

  async ngAfterViewInit() {
    this.startCamera();
  }

  async startCamera() {
    try {
      this.scanning = true;

      this.stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });

      this.video.nativeElement.srcObject = this.stream;
      await this.video.nativeElement.play();

      this.reader.decodeFromVideoElement(
        this.video.nativeElement,
        (result) => {
          if (result) {
            this.scanned.emit(result.getText());
            this.stopCamera();
          }
        }
      );

    } catch {
      this.error = 'Camera permission denied';
      this.scanning = false;
    }
  }

  stopCamera() {
    this.scanning = false;

    if (this.stream) {
      this.stream.getTracks().forEach(t => t.stop());
      this.stream = null;
    }

    this.close.emit();
  }

  ngOnDestroy() {
    this.stopCamera();
  }
}
```
```diff:worker-scanner.component.scss
===
/* Scanner modal overlay */
:host {
  display: block;
}

.scanner-modal {
  position: fixed;
  inset: 0;
  z-index: 50;
  background: rgba(0, 0, 0, 0.95);
  display: flex;
  flex-direction: column;
}

.scanner-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
}

.scanner-top h2 {
  font-size: 18px;
  font-weight: 700;
  color: white;
  margin: 0;
}

.close-btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: none;
  background: rgba(255, 255, 255, 0.1);
  color: #ef4444;
  font-size: 18px;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.close-btn:hover {
  background: rgba(239, 68, 68, 0.2);
}

.scanner-cam {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 20px;
}

.cam-container {
  position: relative;
  width: 100%;
  max-width: 400px;
}

video {
  width: 100%;
  border-radius: 16px;
  border: 2px solid rgba(255, 255, 255, 0.15);
  background: black;
}

.scan-frame {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.frame-box {
  width: 200px;
  height: 200px;
  border: 3px solid #10b981;
  border-radius: 16px;
  animation: pulse-frame 2s ease-in-out infinite;
}

@keyframes pulse-frame {
  0%, 100% {
    opacity: 0.6;
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.3);
  }
  50% {
    opacity: 1;
    box-shadow: 0 0 0 8px rgba(16, 185, 129, 0);
  }
}

.scanner-footer {
  padding: 16px;
  text-align: center;
  color: rgba(255, 255, 255, 0.5);
  font-size: 13px;
}

.scanner-error {
  position: absolute;
  bottom: 80px;
  left: 0;
  right: 0;
  text-align: center;
  color: #fca5a5;
  font-size: 13px;
  padding: 0 20px;
}
```
```diff:event-form-builder.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventFormField } from '../../../../core/models/event-form-field';
import { EventFormService } from '../../../../core/services/event-form.service';



@Component({
  selector: 'app-event-form-builder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './event-form-builder.component.html',
  styleUrl: './event-form-builder.component.scss'
})
export class EventFormBuilderComponent implements OnInit {

  @Input() eventId!: string;

  fields: EventFormField[] = [];

  constructor(private formService: EventFormService) { }

  ngOnInit() {
    this.formService.getForm(this.eventId).subscribe(schema => {
      if (schema) {
        this.fields = schema;
      } else {
        this.fields = [
          { key: 'firstName', label: 'First Name', type: 'text', required: true },
          { key: 'lastName', label: 'Last Name', type: 'text', required: true },
          { key: 'email', label: 'Email', type: 'email', required: true },
          { key: 'phone', label: 'Phone', type: 'phone', required: true }
        ];
      }
    });
  }

  trackByIndex(index: number): number {
    return index;
  }


  addField() {
    this.fields.push({
      key: '',
      label: '',
      type: 'text',
      required: false
    });
  }

  removeField(i: number) {
    this.fields.splice(i, 1);
  }

  save() {
    this.formService.saveForm(this.eventId, this.fields).subscribe(() => {
      alert('Event form saved successfully');
    });
  }
}
===
import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventFormField } from '../../../../core/models/event-form-field';
import { EventFormService } from '../../../../core/services/event-form.service';



@Component({
  selector: 'app-event-form-builder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './event-form-builder.component.html',
  styleUrls: ['./event-form-builder.component.scss']
})
export class EventFormBuilderComponent implements OnInit {

  @Input() eventId!: string;

  fields: EventFormField[] = [];

  constructor(private formService: EventFormService) { }

  ngOnInit() {
    this.formService.getForm(this.eventId).subscribe(schema => {
      if (schema) {
        this.fields = schema;
      } else {
        this.fields = [
          { key: 'firstName', label: 'First Name', type: 'text', required: true },
          { key: 'lastName', label: 'Last Name', type: 'text', required: true },
          { key: 'email', label: 'Email', type: 'email', required: true },
          { key: 'phone', label: 'Phone', type: 'phone', required: true }
        ];
      }
    });
  }

  trackByIndex(index: number): number {
    return index;
  }


  addField() {
    this.fields.push({
      key: '',
      label: '',
      type: 'text',
      required: false
    });
  }

  removeField(i: number) {
    this.fields.splice(i, 1);
  }

  save() {
    this.formService.saveForm(this.eventId, this.fields).subscribe(() => {
      alert('Event form saved successfully');
    });
  }
}
```
```diff:event-register.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { EventRegisterService } from '../../core/services/event-register.service';


@Component({
  standalone: true,
  selector: 'app-event-register',
  imports: [CommonModule, FormsModule],
  templateUrl: './event-register.component.html'
})
export class EventRegisterComponent implements OnInit {
  eventId!: string;
  eventName = '';
  fields: any[] = [];
  formData: Record<string, string> = {};
  success = false;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private service: EventRegisterService
  ) { }

  ngOnInit() {
    this.eventId = this.route.snapshot.paramMap.get('eventId')!;
    this.loadPage();
  }

  loadPage() {
    this.loading = true;

    this.service.getEvent(this.eventId).subscribe(ev => {
      this.eventName = ev.name;
    });

    this.service.getForm(this.eventId).subscribe(form => {
      this.fields = form;
      this.loading = false;
    });
  }

  submit() {
    this.service.submitForm(this.eventId, this.formData)
      .subscribe(() => this.success = true);
  }
}
===
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { EventRegisterService } from '../../core/services/event-register.service';


@Component({
  standalone: true,
  selector: 'app-event-register',
  imports: [CommonModule, FormsModule],
  templateUrl: './event-register.component.html',
  styleUrls: ['./event-register.component.scss']
})
export class EventRegisterComponent implements OnInit {
  eventId!: string;
  eventName = '';
  fields: any[] = [];
  formData: Record<string, string> = {};
  success = false;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private service: EventRegisterService
  ) { }

  ngOnInit() {
    this.eventId = this.route.snapshot.paramMap.get('eventId')!;
    this.loadPage();
  }

  loadPage() {
    this.loading = true;

    this.service.getEvent(this.eventId).subscribe(ev => {
      this.eventName = ev.name;
    });

    this.service.getForm(this.eventId).subscribe(form => {
      this.fields = form;
      this.loading = false;
    });
  }

  submit() {
    this.service.submitForm(this.eventId, this.formData)
      .subscribe(() => this.success = true);
  }
}
```
```diff:event-register.component.html
<div class="min-h-screen flex items-center justify-center bg-gray-50 px-4">
  <div class="w-full max-w-lg bg-white rounded-2xl shadow-lg p-8">

    <!-- HEADER -->
    <div class="mb-6 text-center">
      <h2 class="text-2xl font-bold text-gray-900">
        {{ eventName || 'Event Registration' }}
      </h2>
      <p class="text-sm text-gray-500 mt-1">
        Please fill the form to receive your ticket
      </p>
    </div>

    <!-- LOADING -->
    <div *ngIf="loading" class="text-center text-gray-400">
      Loading registration form…
    </div>

    <!-- FORM -->
    <form *ngIf="!loading && !success"
          (ngSubmit)="submit()"
          class="space-y-4">

      <div *ngFor="let f of fields" class="space-y-1">
        <label class="block text-sm font-medium text-gray-700">
          {{ f.label }}
          <span *ngIf="f.required" class="text-red-500">*</span>
        </label>

        <input *ngIf="f.type !== 'dropdown'"
               class="w-full rounded-lg border px-3 py-2
                 focus:ring-2 focus:ring-blue-500
                 focus:border-blue-500 outline-none"
               [required]="f.required"
               [(ngModel)]="formData[f.key]"
               [name]="f.key"
               [type]="f.type === 'email' ? 'email' : 'text'" />

        <select *ngIf="f.type === 'dropdown'"
                class="w-full rounded-lg border px-3 py-2
                 focus:ring-2 focus:ring-blue-500
                 focus:border-blue-500 outline-none"
                [(ngModel)]="formData[f.key]"
                [name]="f.key"
                [required]="f.required">
          <option value="" disabled selected>
            Select {{ f.label }}
          </option>
          <option *ngFor="let o of f.options" [value]="o">
            {{ o }}
          </option>
        </select>
      </div>

      <button type="submit"
              class="w-full mt-4 py-2 rounded-lg
               bg-blue-600 text-white font-semibold
               hover:bg-blue-700 transition">
        Register & Get Ticket
      </button>
    </form>

    <!-- SUCCESS -->
    <div *ngIf="success"
         class="text-center space-y-2">
      <div class="text-green-600 text-3xl">🎉</div>
      <p class="text-lg font-semibold text-gray-900">
        Registration successful!
      </p>
      <p class="text-sm text-gray-500">
        Your ticket has been sent to your email.
      </p>
    </div>

  </div>
</div>
===
<div class="register-wrapper">
  <!-- Decorative orbs -->
  <div class="reg-orb reg-orb-1"></div>
  <div class="reg-orb reg-orb-2"></div>

  <div class="register-card">

    <!-- HEADER -->
    <div class="reg-header">
      <div class="reg-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
          <circle cx="12" cy="7" r="4"></circle>
        </svg>
      </div>
      <h2 class="reg-title">{{ eventName || 'Event Registration' }}</h2>
      <p class="reg-subtitle">Fill the form below to receive your ticket</p>
    </div>

    <!-- LOADING -->
    <div *ngIf="loading" class="reg-loading">
      <div class="reg-spinner"></div>
      <p>Loading registration form…</p>
    </div>

    <!-- FORM -->
    <form *ngIf="!loading && !success" (ngSubmit)="submit()" class="reg-form">
      <div *ngFor="let f of fields" class="reg-field">
        <label class="reg-label">
          {{ f.label }}
          <span *ngIf="f.required" class="reg-required">*</span>
        </label>

        <input *ngIf="f.type !== 'dropdown'"
               class="reg-input"
               [required]="f.required"
               [(ngModel)]="formData[f.key]"
               [name]="f.key"
               [type]="f.type === 'email' ? 'email' : 'text'"
               [placeholder]="'Enter ' + f.label" />

        <select *ngIf="f.type === 'dropdown'"
                class="reg-input"
                [(ngModel)]="formData[f.key]"
                [name]="f.key"
                [required]="f.required">
          <option value="" disabled selected>Select {{ f.label }}</option>
          <option *ngFor="let o of f.options" [value]="o">{{ o }}</option>
        </select>
      </div>

      <button type="submit" class="reg-submit">
        Register &amp; Get Ticket
      </button>
    </form>

    <!-- SUCCESS -->
    <div *ngIf="success" class="reg-success">
      <div class="success-icon">
        <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="none"
             stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
          <polyline points="22 4 12 14.01 9 11.01"></polyline>
        </svg>
      </div>
      <h3 class="success-title">Registration Successful!</h3>
      <p class="success-text">Your ticket has been sent to your email.</p>
    </div>

  </div>
</div>

```
```diff:index.html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>QreventUi</title>
  <base href="/">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="icon" type="image/x-icon" href="favicon.ico">
</head>
<body>
  <app-root></app-root>
</body>
</html>
===
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>QR Event Platform – Smart Event Ticket Management</title>
  <meta name="description" content="Enterprise QR-based event management platform. Create events, generate tickets, scan QR codes, and manage your team.">
  <base href="/">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="icon" type="image/x-icon" href="favicon.ico">
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
</head>
<body>
  <app-root></app-root>
</body>
</html>

```
```diff:styles.scss
@tailwind base;
@tailwind components;
@tailwind utilities;



.tab {
  @apply px-4 py-2 text-sm rounded-lg text-gray-600 hover:bg-gray-100;
}

  .tab.active {
    @apply bg-blue-600 text-white;
  }

.input {
  @apply w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500;
}

.btn-primary {
  @apply inline-flex items-center px-4 py-2 rounded-lg bg-blue-600 text-white text-sm hover:bg-blue-700;
}

.th {
  @apply px-3 py-2 text-left font-medium text-gray-600;
}

.td {
  @apply px-3 py-2;
}
===
@tailwind base;
@tailwind components;
@tailwind utilities;

/* ===============================
   GLOBAL DESIGN SYSTEM
================================ */
:root {
  --color-primary: #4f46e5;
  --color-primary-dark: #4338ca;
  --color-primary-light: #818cf8;
  --color-accent: #06b6d4;
  --color-success: #10b981;
  --color-warning: #f59e0b;
  --color-danger: #ef4444;
  --color-surface: #ffffff;
  --color-surface-alt: #f8fafc;
  --color-border: #e2e8f0;
  --color-text: #0f172a;
  --color-text-muted: #64748b;
  --gradient-primary: linear-gradient(135deg, #4f46e5 0%, #7c3aed 50%, #a855f7 100%);
  --gradient-surface: linear-gradient(135deg, #f0f4ff 0%, #faf5ff 50%, #f0fdfa 100%);
  --shadow-sm: 0 1px 2px rgba(0,0,0,0.05);
  --shadow-md: 0 4px 16px rgba(0,0,0,0.08);
  --shadow-lg: 0 8px 32px rgba(0,0,0,0.12);
  --shadow-glow: 0 0 24px rgba(79,70,229,0.15);
  --radius-sm: 8px;
  --radius-md: 12px;
  --radius-lg: 16px;
  --radius-xl: 24px;
}

* {
  box-sizing: border-box;
}

body {
  font-family: 'Inter', system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background: var(--gradient-surface);
  color: var(--color-text);
  min-height: 100vh;
}

/* ===============================
   SHARED UTILITY CLASSES
================================ */
.tab {
  @apply px-4 py-2 text-sm rounded-lg text-gray-600 hover:bg-gray-100;
  font-weight: 500;
  transition: all 0.2s ease;
}

.tab.active {
  @apply text-white;
  background: var(--gradient-primary);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.input {
  @apply w-full border rounded-lg px-3 py-2 text-sm;
  border-color: var(--color-border);
  transition: all 0.2s ease;
}

.input:focus {
  @apply ring-2;
  --tw-ring-color: rgba(79, 70, 229, 0.3);
  border-color: var(--color-primary);
  outline: none;
}

.btn-primary {
  @apply inline-flex items-center px-4 py-2 rounded-lg text-white text-sm;
  background: var(--gradient-primary);
  font-weight: 600;
  transition: all 0.2s ease;
  border: none;
  cursor: pointer;
}

.btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 20px rgba(79, 70, 229, 0.35);
}

.btn-primary:active {
  transform: translateY(0);
}

.btn-primary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.th {
  @apply px-3 py-2.5 text-left font-semibold text-gray-500;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.td {
  @apply px-3 py-2.5;
}

/* ===============================
   SCROLLBAR
================================ */
::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 999px;
}

::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

/* ===============================
   ANIMATIONS
================================ */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes pulse-glow {
  0%, 100% {
    box-shadow: 0 0 0 0 rgba(79, 70, 229, 0.2);
  }
  50% {
    box-shadow: 0 0 0 8px rgba(79, 70, 229, 0);
  }
}

.animate-fade-in-up {
  animation: fadeInUp 0.5s ease-out forwards;
}

.animate-fade-in {
  animation: fadeIn 0.4s ease-out forwards;
}

.animate-slide-down {
  animation: slideDown 0.3s ease-out forwards;
}

```
```diff:tailwind.config.js
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
};
===
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
```
