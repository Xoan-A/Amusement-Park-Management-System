namespace Models.In;

public class StrategyRequest
{
    public Guid UserId { get; set; }
    public Guid AttractionId { get; set; }
    public bool IsSpecialEvent { get; set; }
    public DateTime? EnterDate { get; set; }
}