namespace QREventPlatform.Advanced.Models;

public class Ticket
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Code { get; set; } = "";
    public string QrUrl { get; set; } = "";
    public bool IsUsed { get; set; }
}
    