namespace Models.In;

public class AttractionRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public int MinAge { get; set; }
    public int MaxCapacity { get; set; }
    public int? CurrentCapacity { get; set; }
}