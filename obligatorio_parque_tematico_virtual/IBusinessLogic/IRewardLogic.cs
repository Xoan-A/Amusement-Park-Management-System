using Domain;

namespace IBusinessLogic
{
    public interface IRewardLogic
    {
        Reward CreateReward(Reward reward);
        List<Reward> GetAllRewards();
        Reward GetRewardById(Guid id);
        Reward UpdateReward(Guid id, Reward reward);
        void DeleteReward(Guid id);
        List<Reward> GetAvailableRewards();
        List<Reward> GetRewardsByMembershipLevel(MembershipLevel? level);
    }
}
