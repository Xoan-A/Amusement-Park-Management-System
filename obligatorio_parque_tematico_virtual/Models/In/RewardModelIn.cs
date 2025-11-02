using Domain;

namespace Models.In;

public class RewardModelIn
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int PointsCost { get; set; }
    public int AvailableQuantity { get; set; }
    public MembershipLevel? RequiredMembershipLevel { get; set; }
}
