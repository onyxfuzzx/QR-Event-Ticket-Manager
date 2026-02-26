using System.ComponentModel.DataAnnotations;

namespace QREventPlatform.Advanced.Models;

public class BulkAssignRequest
{
    [Required]
    public Guid EventId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one worker is required")]
    public List<Guid> WorkerIds { get; set; } = new();
}
