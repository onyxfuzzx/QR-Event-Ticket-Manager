namespace QREventPlatform.Advanced.Models
{
    public class EventFormField
    {
        public string Key { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Type { get; set; } = default!;
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
    }

}
