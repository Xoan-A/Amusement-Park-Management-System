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

        public RewardModelOut CreateReward(RewardModelIn rewardIn)
        {
            if (rewardIn == null)
            {
                throw new ArgumentNullException(nameof(rewardIn));
            }

            Reward? existingReward = _rewardRepository.GetByName(rewardIn.Name);
            if (existingReward != null)
            {
                throw new ArgumentException($"A reward with the name '{rewardIn.Name}' already exists");
            }

            MembershipLevel? membershipLevel = rewardIn.RequiredMembershipLevel.HasValue
                ? (MembershipLevel)rewardIn.RequiredMembershipLevel.Value
                : null;

            Reward reward = new Reward
            {
                Id = Guid.NewGuid(),
                Name = rewardIn.Name,
                Description = rewardIn.Description,
                PointsCost = rewardIn.PointsCost,
                AvailableQuantity = rewardIn.AvailableQuantity,
                RequiredMembershipLevel = membershipLevel
            };

            _rewardRepository.Create(reward);

            return MapToModelOut(reward);
        }

        public List<RewardModelOut> GetAllRewards()
        {
            List<Reward> rewards = _rewardRepository.GetAll();
            return rewards.Select(MapToModelOut).ToList();
        }

        public RewardModelOut GetRewardById(Guid id)
        {
            Reward? reward = _rewardRepository.GetById(id);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            return MapToModelOut(reward);
        }

        public RewardModelOut UpdateReward(Guid id, RewardModelIn rewardIn)
        {
            Reward? existingReward = _rewardRepository.GetById(id);
            if (existingReward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            Reward? rewardWithSameName = _rewardRepository.GetByName(rewardIn.Name);
            if (rewardWithSameName != null && rewardWithSameName.Id != id)
            {
                throw new ArgumentException($"A reward with the name '{rewardIn.Name}' already exists");
            }

            existingReward.Name = rewardIn.Name;
            existingReward.Description = rewardIn.Description;
            existingReward.PointsCost = rewardIn.PointsCost;
            existingReward.AvailableQuantity = rewardIn.AvailableQuantity;
            existingReward.RequiredMembershipLevel = rewardIn.RequiredMembershipLevel.HasValue
                ? (MembershipLevel)rewardIn.RequiredMembershipLevel.Value
                : null;

            _rewardRepository.Update(existingReward);

            return MapToModelOut(existingReward);
        }

        public void DeleteReward(Guid id)
        {
            Reward? reward = _rewardRepository.GetById(id);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{id}' not found");
            }

            _rewardRepository.Delete(id);
        }

        public List<RewardModelOut> GetAvailableRewards()
        {
            List<Reward> rewards = _rewardRepository.GetAvailableRewards();
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
                RequiredMembershipLevel = reward.RequiredMembershipLevel.HasValue
                    ? (int)reward.RequiredMembershipLevel.Value
                    : null,
                IsAvailable = reward.IsAvailable()
            };
        }
    }
}