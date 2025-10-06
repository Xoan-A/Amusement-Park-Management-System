using Domain;

namespace BusinessLogic;

public class StrategyRequest
{
    public User User { get; set; }
    public Attraction Attraction { get; set; }
    public bool IsSepcialEvent { get; set; }
    public DateTime? EnterDate { get; set; }
}