namespace Models.Out;

public class ScoreHistoryModelOut
{
    public Guid Id { get; set; }
    public Guid VisitorId { get; set; }
    public string? VisitorName { get; set; }
    public int Points { get; set; }
    public string Origin { get; set; }
    public string Description { get; set; }
    public string StrategyName { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}
