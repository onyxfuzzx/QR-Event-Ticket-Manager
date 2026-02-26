using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.DTOs;
using QREventPlatform.Advanced.Enums;
using QREventPlatform.Advanced.Extensions;
using QREventPlatform.Advanced.Models;
using static QRCoder.PayloadGenerator;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly DapperContext _ctx;

    public AdminController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    // =========================
    // WORKERS
    // =========================

    [HttpPost("create-worker")]
    public IActionResult CreateWorker(CreateWorkerRequest req)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var exists = db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Users WHERE Email = @Email",
            new { req.Email });

        if (exists > 0)
            return BadRequest("Email already exists");

        var workerId = Guid.NewGuid();

        db.Execute("""
    INSERT INTO Users
    (Id, Name, Email, PasswordHash, Role, CreatedByAdminId, IsActive, CreatedAt)
    VALUES
    (@Id, @Name, @Email, @Hash, @Role, @AdminId, 1, GETUTCDATE())
""", new
        {
            Id = workerId,
            Name = req.Name,
            Email = req.Email,
            Hash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = (int)Role.Worker,
            AdminId = adminId
        });


        return Ok(new { workerId });
    }




    [HttpGet("workers")]
    public IActionResult GetWorkers()
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();
        var workerRole = (int)Role.Worker;

        var workers = db.Query(
            """
    
            SELECT Id,
            Name, Email,
            IsActive FROM Users WHERE Role = @Role AND CreatedByAdminId = @AdminId
    
            """, new
          { Role = workerRole, AdminId = adminId });

        return Ok(workers);
    }

    [HttpDelete("workers/{workerId}")]
    public IActionResult DeleteWorker(Guid workerId)
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        var adminId = User.GetUserId();
        using var tx = db.BeginTransaction();
        var workerRole = (int)Role.Worker;

        try
        {
            db.Execute("""
                UPDATE EventWorkers
                SET IsActive = 0
                WHERE WorkerId = @WorkerId
            """, new { WorkerId = workerId }, tx);

            var rows = db.Execute("""
                UPDATE Users
                SET IsActive = 0
                WHERE Id = @WorkerId
                 AND Role = @Role
                AND CreatedByAdminId = @AdminId
                """, new
            {
                WorkerId = workerId,
                Role = workerRole,
                AdminId = adminId
            }, tx);

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

    // =========================
    // EVENTS
    // =========================

    [Authorize(Roles = "Admin")]
    [HttpDelete("{eventId}")]
    public IActionResult DeleteEvent(Guid eventId)
    {
        using var db = _ctx.CreateConnection();
        db.Open();

        using var tx = db.BeginTransaction();

        try
        {
            // 1️⃣ Soft-unassign all workers
            db.Execute("""
            UPDATE EventWorkers
            SET IsActive = 0
            WHERE EventId = @EventId
        """, new { EventId = eventId }, tx);

            // 2️⃣ Soft-delete event (NOT DELETE)
            var rows = db.Execute("""
            UPDATE Events
            SET IsActive = 0
            WHERE Id = @EventId
        """, new { EventId = eventId }, tx);

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

    // =========================
    // EVENT ↔ WORKER
    // =========================

    [HttpPost("assign-worker")]
    public IActionResult AssignWorker([FromBody] AssignWorkerRequest req)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var ownsEvent = db.ExecuteScalar<int>("""
    SELECT COUNT(*)
    FROM Events
    WHERE Id = @EventId
      AND CreatedByAdminId = @AdminId
      AND IsActive = 1
""", new { req.EventId, AdminId = adminId });

        if (ownsEvent == 0)
            return Forbid();

        var existing = db.QuerySingleOrDefault<Guid?>(@"
            SELECT Id FROM EventWorkers
            WHERE EventId = @EventId AND WorkerId = @WorkerId
        ", req);

        if (existing.HasValue)
        {
            db.Execute("""
                UPDATE EventWorkers
                SET IsActive = 1
                WHERE Id = @Id
            """, new { Id = existing.Value });

            return Ok();
        }

        db.Execute("""
    INSERT INTO EventWorkers
(Id, EventId, WorkerId, IsActive, AssignedAt)
VALUES
(@Id, @EventId, @WorkerId, 1, GETUTCDATE())
""", new
        {
            Id = Guid.NewGuid(),
            req.EventId,
            req.WorkerId
        });


        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("assign-workers-bulk")]
    public IActionResult BulkAssignWorkers([FromBody] BulkAssignRequest req)
    {
        using var db = _ctx.CreateConnection();

        // 🔒 Validate Event
        var eventValid = db.ExecuteScalar<int>(
            @"
        SELECT COUNT(*)
        FROM Events
        WHERE Id = @EventId
          AND IsActive = 1
    ",
            new { req.EventId });

        if (eventValid == 0)
            return BadRequest("Event not found or inactive");

        int assigned = 0;
        int skipped = 0;

        foreach (var workerId in req.WorkerIds)
        {
            // 🔒 Validate Worker
            var workerRole = (int)Role.Worker;
            var workerValid = db.ExecuteScalar<int>(
                @"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Id = @WorkerId
                      AND Role = @Role
                      AND IsActive = 1
                        ",
                new { WorkerId = workerId, Role = workerRole });

            if (workerValid == 0)
            {
                skipped++;
                continue;
            }

            // 🔍 Check existing assignment
            var existing = db.QuerySingleOrDefault<EventWorkerStateDto>(
                @"
    SELECT Id, IsActive
    FROM EventWorkers
    WHERE EventId = @EventId
      AND WorkerId = @WorkerId
",
                new { req.EventId, WorkerId = workerId });

            if (existing != null)
            {
                // Already active → skip
                if (existing.IsActive == true)
                {
                    skipped++;
                    continue;
                }

                // Reactivate
                db.Execute("""
                UPDATE EventWorkers
                SET IsActive = 1,
                    AssignedAt = GETUTCDATE()
                WHERE Id = @Id
            """, new { Id = existing.Id });

                assigned++;
                continue;
            }

            // Fresh assignment
            db.Execute("""
            INSERT INTO EventWorkers
            (Id, EventId, WorkerId, IsActive)
            VALUES
            (@Id, @EventId, @WorkerId, 1)
        """, new
            {
                Id = Guid.NewGuid(),
                req.EventId,
                WorkerId = workerId
            });

            assigned++;
        }

        return Ok(new { assigned, skipped });
    }

    [HttpGet("events/{eventId}/workers")]
    public IActionResult GetEventWorkers(Guid eventId)
    {
        using var db = _ctx.CreateConnection();

        var workers = db.Query("""
            SELECT
                ew.Id AS AssignmentId,
                u.Id AS WorkerId,
                u.Name,
                u.Email,
                ew.AssignedAt
            FROM EventWorkers ew
            JOIN Users u ON u.Id = ew.WorkerId
            WHERE ew.EventId = @EventId
              AND ew.IsActive = 1
            ORDER BY ew.AssignedAt DESC
        """, new { EventId = eventId });

        return Ok(workers);
    }

    [HttpDelete("event-workers/{assignmentId}")]
    public IActionResult UnassignWorker(Guid assignmentId)
    {
        using var db = _ctx.CreateConnection();

        var adminId = User.GetUserId();

        var ownsAssignment = db.ExecuteScalar<int>("""
    SELECT COUNT(*)
    FROM EventWorkers ew
    JOIN Events e ON e.Id = ew.EventId
    WHERE ew.Id = @AssignmentId
      AND e.CreatedByAdminId = @AdminId
""", new { AssignmentId = assignmentId, AdminId = adminId });

        if (ownsAssignment == 0)
            return Forbid();

        db.Execute("""
            UPDATE EventWorkers
            SET IsActive = 0
            WHERE Id = @Id
        """, new { Id = assignmentId });

        return Ok(new { success = true });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("workers/{workerId}/restore")]
    public IActionResult RestoreWorker(Guid workerId)
    {
        using var db = _ctx.CreateConnection();
        db.Open(); // 🔑 REQUIRED
        var adminId = User.GetUserId();
        var workerRole = (int)Role.Worker;

        using var tx = db.BeginTransaction();

        var rows = db.Execute("""
        UPDATE Users
        SET IsActive = 1
        WHERE Id = @WorkerId
          AND Role = @Role
          AND CreatedByAdminId = @AdminId
    """, new
        {
            WorkerId = workerId,
            Role = workerRole,
            AdminId = adminId
        }, tx);

        if (rows == 0)
        {
            tx.Rollback();
            return NotFound();
        }

        // 🔁 Restore assignments
        db.Execute("""
        UPDATE EventWorkers
        SET IsActive = 1
        WHERE WorkerId = @WorkerId
    """, new { WorkerId = workerId }, tx);

        tx.Commit();
        return Ok(new { success = true });
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("workers/deleted")]
    public IActionResult GetDeletedWorkers()
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var workers = db.Query("""
        SELECT
            Id,
            Name,
            Email,
            CreatedAt
        FROM Users
        WHERE Role = 2
          AND IsActive = 0
          AND CreatedByAdminId = @AdminId
        ORDER BY CreatedAt DESC
    """, new { AdminId = adminId });

        return Ok(workers);
    }

}
