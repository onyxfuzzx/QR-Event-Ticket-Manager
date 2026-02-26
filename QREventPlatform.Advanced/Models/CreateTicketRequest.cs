using System.ComponentModel.DataAnnotations;

namespace QREventPlatform.Advanced.Models;

public class CreateTicketRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
