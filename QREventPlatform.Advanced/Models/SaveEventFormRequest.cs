namespace QREventPlatform.Advanced.Models
{
    public class SaveEventFormRequest
    {
        public Guid EventId { get; set; }
        public List<EventFormField> Fields { get; set; } = [];
    }

}
