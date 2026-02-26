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
