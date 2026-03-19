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
    private readonly IConfiguration _config;

    public PublicEventController(DapperContext ctx, EmailService email, IConfiguration config)
    {
        _ctx = ctx;
        _email = email;
        _config = config;
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
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Code = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            QrUrl = $"{baseUrl}/qr/{Guid.NewGuid()}"
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
