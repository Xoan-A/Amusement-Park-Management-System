using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic
{
    public class RewardLogic : IRewardLogic
    {
        private readonly IRewardRepository _rewardRepository;

        public RewardLogic(IRewardRepository rewardRepository)
        {
            _rewardRepository = rewardRepository;
        }

        public async Task<RewardModelOut> CreateReward(RewardModelIn rewardIn)
        {
            if (rewardIn == null)
            {
                throw new ArgumentNullException(nameof(rewardIn));
            }

            Reward? existingReward = await _rewardRepository.GetByNameAsync(rewardIn.Name);
            if (existingReward != null)
            {
                throw new ArgumentException($"A reward with the name '{rewardIn.Name}' already exists");
            }

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = rewardIn.Name,
                Description = rewardIn.Description,
                PointsCost = rewardIn.PointsCost,
                AvailableQuantity = rewardIn.AvailableQuantity,
                RequiredMembershipLevel = rewardIn.RequiredMembershipLevel
            };

            await _rewardRepository.CreateAsync(reward);

            return MapToModelOut(reward);
        }

        public async Task<List<RewardModelOut>> GetAllRewards()
        {
            List<Reward> rewards = await _rewardRepository.GetAllAsync();
            return rewards.Select(MapToModelOut).ToList();
        }

        public async Task<RewardModelOut> GetRewardById(Guid id)
        {
            Reward? reward = await _rewardRepository.GetByIdAsync(id);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            return MapToModelOut(reward);
        }

        public async Task<RewardModelOut> UpdateReward(Guid id, RewardModelIn rewardIn)
        {
            Reward? existingReward = await _rewardRepository.GetByIdAsync(id);
            if (existingReward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            Reward? rewardWithSameName = await _rewardRepository.GetByNameAsync(rewardIn.Name);
            if (rewardWithSameName != null && rewardWithSameName.Id != id)
            {
                throw new ArgumentException($"A reward with the name '{rewardIn.Name}' already exists");
            }

            existingReward.Name = rewardIn.Name;
            existingReward.Description = rewardIn.Description;
            existingReward.PointsCost = rewardIn.PointsCost;
            existingReward.AvailableQuantity = rewardIn.AvailableQuantity;
            existingReward.RequiredMembershipLevel = rewardIn.RequiredMembershipLevel;

            await _rewardRepository.UpdateAsync(existingReward);

            return MapToModelOut(existingReward);
        }

        public async Task DeleteReward(Guid id)
        {
            Reward? reward = await _rewardRepository.GetByIdAsync(id);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            await _rewardRepository.DeleteAsync(id);
        }

        public async Task<List<RewardModelOut>> GetAvailableRewards()
        {
            List<Reward> rewards = await _rewardRepository.GetAvailableRewardsAsync();
            return rewards.Select(MapToModelOut).ToList();
        }
        
        private RewardModelOut MapToModelOut(Reward reward)
        {
            return new RewardModelOut
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description,
                PointsCost = reward.PointsCost,
                AvailableQuantity = reward.AvailableQuantity,
                RequiredMembershipLevel = reward.RequiredMembershipLevel,
                IsAvailable = reward.IsAvailable()
            };
        }
    }
}