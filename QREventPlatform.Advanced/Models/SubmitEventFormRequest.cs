namespace QREventPlatform.Advanced.Models
{
    public class SubmitEventFormRequest
    {
        public Guid EventId { get; set; }
        public Dictionary<string, string> FormData { get; set; } = [];
    }

}
