using Domain;

namespace Models.Out;

public class AttractionsVisitResponse
{
    public List<(Attraction, int)> AttractionsVisits { get; set; } = new List<(Attraction, int)>();
}