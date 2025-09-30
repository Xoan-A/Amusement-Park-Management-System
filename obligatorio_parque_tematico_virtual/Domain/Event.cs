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
    public List<Attraction> Attractions { get; set; }
    
    public Event()
    {
        this.Id = Guid.NewGuid();
        this.Attractions = new List<Attraction>();
    }

    public void AddAttraction(Attraction attraction)
    {
        this.Attractions.Add(attraction);
    }

    public void RemoveAttraction(Attraction attraction)
    {
        this.Attractions.Remove(attraction);
    }
}