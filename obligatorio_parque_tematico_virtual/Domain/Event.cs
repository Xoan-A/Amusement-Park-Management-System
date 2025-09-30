namespace Domain;

public class Event
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentCapacity { get; set; }
    public decimal Cost { get; set; }
    public List<EventAttraction> Attractions { get; set; }

    public Event()
    {
        this.Id = Guid.NewGuid();
        this.Attractions = new List<EventAttraction>();
    }

    public void AddAttraction(Attraction attraction)
    {
        var eventAttraction = new EventAttraction
        {
            EventId = this.Id,
            Event = this,
            AttractionId = attraction.Id,
            Attraction = attraction
        };
        this.Attractions.Add(eventAttraction);
    }

    public void RemoveAttraction(Attraction attraction)
    {
        EventAttraction eventAttraction = this.Attractions
            .FirstOrDefault(ea => ea.AttractionId == attraction.Id);

        this.Attractions.Remove(eventAttraction);
    }

    public bool HasAttraction(Guid attractionId)
    {
        return this.Attractions.Any(ea => ea.AttractionId == attractionId);
    }
}