using System.ComponentModel.DataAnnotations;

namespace QREventPlatform.Advanced.Models;

public class ValidateTicketRequest
{
    [Required]
    [MinLength(8)]
    public string Code { get; set; } = string.Empty;
}
