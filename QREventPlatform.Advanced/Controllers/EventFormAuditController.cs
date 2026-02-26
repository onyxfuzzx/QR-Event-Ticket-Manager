using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using System.Text.Json;

[ApiController]
[Route("api/admin/form-audit")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class EventFormAuditController : ControllerBase
{
    private readonly DapperContext _ctx;

    public EventFormAuditController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetFormSubmissions(Guid eventId)
    {
        using var db = _ctx.CreateConnection();

        var rows = await db.QueryAsync(
            """
            SELECT Data, CreatedAt
            FROM EventFormSubmissions
            WHERE EventId = @eventId
            ORDER BY CreatedAt DESC
            """,
            new { eventId }
        );

        var result = rows.Select(r => new
        {
            createdAt = r.CreatedAt,
            data = JsonSerializer.Deserialize<Dictionary<string, string>>(r.Data)
        });

        return Ok(result);
    }
}
