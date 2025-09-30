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
}