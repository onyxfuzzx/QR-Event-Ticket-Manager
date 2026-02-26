using System.ComponentModel.DataAnnotations;

namespace QREventPlatform.Advanced.Models;

public class CreateEventRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }
}
