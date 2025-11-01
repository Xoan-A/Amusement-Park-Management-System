using BusinessLogic.Specifications;
using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class RedemptionLogic : IRedemptionLogic
    {
        private readonly IRewardRepository _rewardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRedemptionHistoryRepository _redemptionHistoryRepository;

        public RedemptionLogic(
            IRewardRepository rewardRepository,
            IUserRepository userRepository,
            IRedemptionHistoryRepository redemptionHistoryRepository)
        {
            _rewardRepository = rewardRepository;
            _userRepository = userRepository;
            _redemptionHistoryRepository = redemptionHistoryRepository;
        }

        public RedemptionHistory RedeemReward(Guid visitorId, Guid rewardId)
        {
            var visitor = _userRepository.GetById(visitorId).Result;
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID '{visitorId}' not found");
            }

            var reward = _rewardRepository.GetById(rewardId);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{rewardId}' not found");
            }

            ValidateRedemptionEligibility(visitor, reward);

            visitor.Score -= reward.PointsCost;
            reward.DecrementQuantity();

            var redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = DateTime.Now,
                PointsSpent = reward.PointsCost
            };

            _redemptionHistoryRepository.Create(redemption);
            _userRepository.Update(visitor).Wait();
            _rewardRepository.Update(reward);

            return redemption;
        }

        public List<RedemptionHistory> GetRedemptionHistory(Guid visitorId)
        {
            return _redemptionHistoryRepository.GetByVisitorId(visitorId);
        }

        public List<RedemptionHistory> GetRedemptionHistoryWithDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo)
        {
            return _redemptionHistoryRepository.GetByVisitorIdWithDateRange(visitorId, dateFrom, dateTo);
        }

        private void ValidateRedemptionEligibility(User visitor, Reward reward)
        {
            var pointsSpec = new HasSufficientPointsSpecification(reward.PointsCost);
            var membershipSpec = new MeetsRequiredMembershipSpecification(reward.RequiredMembershipLevel);
            var availabilitySpec = new RewardIsAvailableSpecification();

            if (!pointsSpec.IsSatisfiedBy(visitor))
            {
                throw new InvalidOperationException(
                    $"Visitor does not have sufficient points. Required: {reward.PointsCost}, Available: {visitor.Score}");
            }

            if (!membershipSpec.IsSatisfiedBy(visitor))
            {
                throw new InvalidOperationException(
                    $"Visitor does not meet the required membership level. Required: {reward.RequiredMembershipLevel}, Current: {visitor.MembershipLevel}");
            }

            if (!availabilitySpec.IsSatisfiedBy(reward))
            {
                throw new InvalidOperationException($"Reward '{reward.Name}' is not available (out of stock)");
            }
        }
    }
}
