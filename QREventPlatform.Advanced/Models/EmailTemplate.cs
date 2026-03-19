using System;

namespace QREventPlatform.Advanced.Models;

public class EmailTemplate
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string LayoutJson { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
