namespace QREventPlatform.Advanced.Models;

public class CreateTicketResponse
{
    public Guid TicketId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string QrUrl { get; set; } = string.Empty;
}
