using Domain;
using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface IRewardLogic
    {
        Task<RewardModelOut> CreateReward(RewardModelIn reward);
        Task<List<RewardModelOut>> GetAllRewards();
        Task<RewardModelOut> GetRewardById(Guid id);
        Task<RewardModelOut> UpdateReward(Guid id, RewardModelIn reward);
        Task DeleteReward(Guid id);
        Task<List<RewardModelOut>> GetAvailableRewards();
        Task<List<RewardModelOut>> GetRewardsByMembershipLevel(MembershipLevel? level);
    }
}
