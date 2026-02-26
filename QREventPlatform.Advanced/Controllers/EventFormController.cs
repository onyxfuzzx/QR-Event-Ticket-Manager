using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QREventPlatform.Advanced.Data;
using System.Text.Json;

[ApiController]
[Route("api/event-forms")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class EventFormsController : ControllerBase
{
    private readonly DapperContext _ctx;

    public EventFormsController(DapperContext ctx)
    {
        _ctx = ctx;
    }

    // ===============================
    // GET FORM
    // ===============================
    [HttpGet("{eventId}")]
    public IActionResult GetForm(Guid eventId)
    {
        using var db = _ctx.CreateConnection();

        var schema = db.ExecuteScalar<string>(
            "SELECT [Schema] FROM EventForms WHERE EventId = @eventId",
            new { eventId });

        if (schema == null)
            return Ok(null);

        return Ok(JsonSerializer.Deserialize<object>(schema));
    }

    // ===============================
    // SAVE / UPDATE FORM
    // ===============================
    [HttpPost("{eventId}")]
    public IActionResult SaveForm(Guid eventId, [FromBody] object schema)
    {
        using var db = _ctx.CreateConnection();

        var json = JsonSerializer.Serialize(schema);

        db.Execute("""
            MERGE EventForms AS target
            USING (SELECT @EventId AS EventId) AS source
            ON target.EventId = source.EventId
            WHEN MATCHED THEN
                UPDATE SET [Schema] = @Schema, UpdatedAt = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (Id, EventId, [Schema], CreatedAt)
                VALUES (NEWID(), @EventId, @Schema, SYSUTCDATETIME());
        """, new
        {
            EventId = eventId,
            Schema = json
        });

        return Ok(new { message = "Form saved successfully" });
    }
}
