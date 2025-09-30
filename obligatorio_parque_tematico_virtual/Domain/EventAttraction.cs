namespace Domain;

public class EventAttraction
{
    public Guid EventId { get; set; }
    public Event Event { get; set; }
    public Guid AttractionId { get; set; }
    public Attraction Attraction { get; set; }
}