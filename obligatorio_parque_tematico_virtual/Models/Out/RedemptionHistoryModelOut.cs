namespace Models.Out;

public class RedemptionHistoryModelOut
{
    public Guid Id { get; set; }
    public Guid VisitorId { get; set; }
    public Guid RewardId { get; set; }
    public DateTime RedeemedAt { get; set; }
    public int PointsSpent { get; set; }
    public string? RewardName { get; set; }
    public string? VisitorName { get; set; }
}
