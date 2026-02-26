using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Enums;
using QREventPlatform.Advanced.Extensions;
using QREventPlatform.Advanced.Models;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/superadmin")]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly DapperContext _ctx;

    public SuperAdminController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    [HttpPost("create-admin")]
    public IActionResult CreateAdmin(CreateUserRequest req)
    {
        using var db = _ctx.CreateConnection();

        var newAdminId = Guid.NewGuid();
        var superAdminId = User.GetUserId();  // ✅ THIS MUST EXIST IN USERS

        db.Execute("""
        INSERT INTO Users
        (Id, Name, Email, PasswordHash, Role, CreatedByAdminId, IsActive)
        VALUES
        (@Id, @Name, @Email, @Hash, @Role, @CreatedBy, 1)
    """, new
        {
            Id = newAdminId,
            req.Name,
            req.Email,
            Hash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = (int)Role.Admin,
            CreatedBy = superAdminId
        });

        return Ok(new { id = newAdminId, name = req.Name, email = req.Email });
    }

    // ============================
    // GET ALL ADMINS
    // ============================
    [HttpGet("admins")]
    public IActionResult GetAdmins()
    {
        using var db = _ctx.CreateConnection();
        var adminRole = (int)Role.Admin;

        var admins = db.Query("""
        SELECT Id, Name, Email, IsActive, CreatedAt
        FROM Users
        WHERE Role = @Role
    """, new { Role = adminRole });

        return Ok(admins);
    }

    // ============================
    // DELETE ADMIN (SOFT DELETE)
    // ============================
    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("admins/{adminId}")]
    public IActionResult DeleteAdmin(Guid adminId)
    {
        using var db = _ctx.CreateConnection();
        db.Open();
        var adminRole = (int)Role.Admin;
        var workerRole = (int)Role.Worker;

        using var tx = db.BeginTransaction();

        try
        {
            // 1️⃣ Disable events created by this admin
            db.Execute("""
            UPDATE Events
            SET IsActive = 0
            WHERE CreatedByAdminId = @AdminId
        """, new { AdminId = adminId }, tx);

            // 2️⃣ Disable workers created by this admin
            db.Execute("""
    UPDATE Users
    SET IsActive = 0
    WHERE CreatedByAdminId = @AdminId
      AND Role = @WorkerRole
""", new
            {
                AdminId = adminId,
                WorkerRole = workerRole
            }, tx);


            // 3️⃣ Disable the admin himself
            var rows = db.Execute("""
    UPDATE Users
    SET IsActive = 0
    WHERE Id = @AdminId
      AND Role = @AdminRole
""", new
            {
                AdminId = adminId,
                AdminRole = adminRole
            }, tx);


            if (rows == 0)
            {
                tx.Rollback();
                return NotFound("Admin not found");
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

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("admins/{adminId}/restore")]
    public IActionResult RestoreAdmin(Guid adminId)
    {
        using var db = _ctx.CreateConnection();
        var adminRole = (int)Role.Admin;
        var workerRole = (int)Role.Worker;
        db.Open(); // 🔑 REQUIRED
        using var tx = db.BeginTransaction();

        var rows = db.Execute("""
        UPDATE Users
        SET IsActive = 1
        WHERE Id = @AdminId
          AND Role = @AdminRole
    """, new { AdminId = adminId, AdminRole = adminRole }, tx);

        if (rows == 0)
        {
            tx.Rollback();
            return NotFound();
        }

        // 🔁 Restore events
        db.Execute("""
        UPDATE Events SET IsActive = 1 WHERE CreatedByAdminId = @AdminId
    """, new { AdminId = adminId }, tx);

        // 🔁 Restore workers
        db.Execute("""
        UPDATE Users
        SET IsActive = 1
        WHERE CreatedByAdminId = @AdminId
          AND Role = @WorkerRole
    """, new { AdminId = adminId, WorkerRole = workerRole }, tx);

        tx.Commit();
        return Ok(new { success = true });
    }


    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("admins/deleted")]
    public IActionResult GetDeletedAdmins()
    {
        using var db = _ctx.CreateConnection();

        var admins = db.Query("""
        SELECT
            Id,
            Name,
            Email,
            CreatedAt
        FROM Users
        WHERE Role = 1
          AND IsActive = 0
        ORDER BY CreatedAt DESC
    """);

        return Ok(admins);
    }

}
