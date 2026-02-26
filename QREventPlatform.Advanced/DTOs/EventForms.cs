namespace QREventPlatform.Advanced.DTOs
{
    public class EventFormFieldDto
    {
        public string Key { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool Required { get; set; }
        public string? Options { get; set; } // JSON array
    }


    public class CreateEventFormRequest
    {
        public List<EventFormFieldDto> Fields { get; set; } = new();
    }

    public class EventFormResponse
    {
        public Guid FormId { get; set; }
        public Guid EventId { get; set; }
        public List<EventFormFieldDto> Fields { get; set; } = new();
    }


}
