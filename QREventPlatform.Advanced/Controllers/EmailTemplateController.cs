using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using QREventPlatform.Advanced.Extensions;
using QREventPlatform.Advanced.Models;
using QREventPlatform.Advanced.Services;

namespace QREventPlatform.Advanced.Controllers;

[ApiController]
[Route("api/admin/events/{eventId}/template")]
[Authorize(Roles = "Admin")]
public class EmailTemplateController : ControllerBase
{
    private readonly DapperContext _ctx;
    private readonly EmailService _email;

    public EmailTemplateController(DapperContext ctx, EmailService email)
    {
        _ctx = ctx;
        _email = email;
    }

    [HttpGet]
    public IActionResult GetTemplate(Guid eventId)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        // 🔒 Verify ownership
        var ownsEvent = db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Events WHERE Id = @Id AND CreatedByAdminId = @AdminId",
            new { Id = eventId, AdminId = adminId });

        if (ownsEvent == 0) return Forbid();

        var template = db.QuerySingleOrDefault<EmailTemplate>(
            "SELECT * FROM EmailTemplates WHERE EventId = @EventId",
            new { EventId = eventId });

        if (template == null)
        {
            // Default template structure
            return Ok(new
            {
                LayoutJson = "[]",
                HtmlContent = ""
            });
        }

        return Ok(template);
    }

    [HttpPost]
    public IActionResult UpsertTemplate(Guid eventId, [FromBody] EmailTemplate model)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        // 🔒 Verify ownership
        var ownsEvent = db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM Events WHERE Id = @Id AND CreatedByAdminId = @AdminId",
            new { Id = eventId, AdminId = adminId });

        if (ownsEvent == 0) return Forbid();

        db.Execute("""
            IF EXISTS (SELECT 1 FROM EmailTemplates WHERE EventId = @EventId)
            BEGIN
                UPDATE EmailTemplates 
                SET LayoutJson = @LayoutJson,
                    HtmlContent = @HtmlContent,
                    UpdatedAt = GETUTCDATE()
                WHERE EventId = @EventId
            END
            ELSE
            BEGIN
                INSERT INTO EmailTemplates (Id, EventId, LayoutJson, HtmlContent)
                VALUES (@Id, @EventId, @LayoutJson, @HtmlContent)
            END
        """, new
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            model.LayoutJson,
            model.HtmlContent
        });

        return Ok(new { success = true });
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestTemplate(Guid eventId, [FromBody] TestEmailRequest req)
    {
        using var db = _ctx.CreateConnection();
        var adminId = User.GetUserId();

        var ev = db.QuerySingleOrDefault(
            "SELECT Name, EventDate, Location FROM Events WHERE Id = @Id", new { Id = eventId });

        await _email.SendCustomTicketAsync(
            req.ToEmail,
            ev?.Name ?? "Test Event",
            "TICKET-123456",
            "https://example.com/qr",
            req.HtmlContent,
            eventDate: ev?.EventDate,
            location: ev?.Location
        );

        return Ok(new { success = true });
    }
}

public class TestEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
}
