namespace Models.Out;

public class RewardModelOut
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int PointsCost { get; set; }
    public int AvailableQuantity { get; set; }
    public int? RequiredMembershipLevel { get; set; }
    public bool IsAvailable { get; set; }
}
