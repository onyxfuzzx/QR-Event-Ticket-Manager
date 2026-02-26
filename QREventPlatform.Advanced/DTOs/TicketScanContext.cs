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
