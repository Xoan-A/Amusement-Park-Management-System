namespace Domain;

public class Attraction
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public AttractionType Type { get; set; }
    public int MinAge { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentCapacity { get; set; }
    public bool IsActive => Incidents.Count == 0;
    public List<String> Incidents { get; set; } = new List<String>();

    public Attraction()
    {
        this.Id = Guid.NewGuid();
    }
    
    public void AddIncident (string incident)
    {
        Incidents.Add(incident);
    }
    
    public void RemoveIncident (string incident)
    {
        Incidents.Remove(incident);
    }
}