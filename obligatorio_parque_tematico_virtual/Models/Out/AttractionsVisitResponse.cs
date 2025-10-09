using Domain;

namespace Models.Out;

public class AttractionsVisitResponse
{
    public List<(AttractionResponse, int)> AttractionsVisits { get; set; } = new List<(AttractionResponse, int)>();
}