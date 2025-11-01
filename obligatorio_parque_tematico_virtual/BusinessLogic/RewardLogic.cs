using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class RewardLogic : IRewardLogic
    {
        private readonly IRewardRepository _rewardRepository;

        public RewardLogic(IRewardRepository rewardRepository)
        {
            _rewardRepository = rewardRepository;
        }

        public Reward CreateReward(Reward reward)
        {
            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }

            var existingReward = _rewardRepository.GetByName(reward.Name);
            if (existingReward != null)
            {
                throw new ArgumentException($"A reward with the name '{reward.Name}' already exists");
            }

            reward.Id = Guid.NewGuid();
            _rewardRepository.Create(reward);

            return reward;
        }

        public List<Reward> GetAllRewards()
        {
            return _rewardRepository.GetAll();
        }

        public Reward GetRewardById(Guid id)
        {
            var reward = _rewardRepository.GetById(id);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            return reward;
        }

        public Reward UpdateReward(Guid id, Reward reward)
        {
            var existingReward = _rewardRepository.GetById(id);
            if (existingReward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            var rewardWithSameName = _rewardRepository.GetByName(reward.Name);
            if (rewardWithSameName != null && rewardWithSameName.Id != id)
            {
                throw new ArgumentException($"A reward with the name '{reward.Name}' already exists");
            }

            existingReward.Name = reward.Name;
            existingReward.Description = reward.Description;
            existingReward.PointsCost = reward.PointsCost;
            existingReward.AvailableQuantity = reward.AvailableQuantity;
            existingReward.RequiredMembershipLevel = reward.RequiredMembershipLevel;

            _rewardRepository.Update(existingReward);

            return existingReward;
        }

        public void DeleteReward(Guid id)
        {
            var reward = _rewardRepository.GetById(id);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            _rewardRepository.Delete(id);
        }

        public List<Reward> GetAvailableRewards()
        {
            return _rewardRepository.GetAvailableRewards();
        }

        public List<Reward> GetRewardsByMembershipLevel(MembershipLevel? level)
        {
            return _rewardRepository.GetRewardsByMembershipLevel(level);
        }
    }
}
