namespace Models.In;

public class EventRequest
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public int MaxCapacity { get; set; }
    public decimal Cost { get; set; }
    public List<Guid> AttractionIds { get; set; }
}