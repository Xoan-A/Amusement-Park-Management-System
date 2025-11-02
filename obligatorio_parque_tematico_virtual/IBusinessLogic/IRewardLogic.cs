using Domain;
using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface IRewardLogic
    {
        RewardModelOut CreateReward(RewardModelIn reward);
        List<RewardModelOut> GetAllRewards();
        RewardModelOut GetRewardById(Guid id);
        RewardModelOut UpdateReward(Guid id, RewardModelIn reward);
        void DeleteReward(Guid id);
        List<RewardModelOut> GetAvailableRewards();
        List<RewardModelOut> GetRewardsByMembershipLevel(MembershipLevel? level);
    }
}
