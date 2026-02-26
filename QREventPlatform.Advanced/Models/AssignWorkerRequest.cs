using System.ComponentModel.DataAnnotations;

namespace QREventPlatform.Advanced.Models;

public class AssignWorkerRequest
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    public Guid WorkerId { get; set; }
}
