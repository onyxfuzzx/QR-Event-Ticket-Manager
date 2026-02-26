namespace QREventPlatform.Advanced.Models;

public class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Location { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsActive { get; set; }
}
