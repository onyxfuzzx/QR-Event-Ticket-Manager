namespace QREventPlatform.Advanced.DTOs;

public class EventInfoDto
{
    public string Name { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string? Location { get; set; }
}
