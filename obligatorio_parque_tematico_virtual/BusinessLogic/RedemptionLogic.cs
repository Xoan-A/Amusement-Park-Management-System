using BusinessLogic.Specifications;
using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.Out;

namespace BusinessLogic
{
    public class RedemptionLogic : IRedemptionLogic
    {
        private readonly IRewardRepository _rewardRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRedemptionHistoryRepository _redemptionHistoryRepository;
        private readonly IDateTimeLogic _dateTimeLogic;
        private readonly IScoreHistoryRepository _scoreHistoryRepository;

        public RedemptionLogic(
            IRewardRepository rewardRepository,
            IUserRepository userRepository,
            IRedemptionHistoryRepository redemptionHistoryRepository,
            IDateTimeLogic dateTimeLogic,
            IScoreHistoryRepository scoreHistoryRepository)
        {
            _rewardRepository = rewardRepository;
            _userRepository = userRepository;
            _redemptionHistoryRepository = redemptionHistoryRepository;
            _dateTimeLogic = dateTimeLogic;
            _scoreHistoryRepository = scoreHistoryRepository;
        }

        public async Task<RedemptionHistoryModelOut> RedeemReward(Guid visitorId, Guid rewardId)
        {
            User? visitor = await _userRepository.GetById(visitorId);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID '{visitorId}' not found");
            }

            Reward? reward = await _rewardRepository.GetByIdAsync(rewardId);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{rewardId}' not found");
            }

            ValidateRedemptionEligibility(visitor, reward);

            visitor.Score -= reward.PointsCost;
            reward.DecrementQuantity();

            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime().Result;

            RedemptionHistory redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = currentDateTime,
                PointsSpent = reward.PointsCost
            };

            await _redemptionHistoryRepository.CreateAsync(redemption);
            await _userRepository.Update(visitor);
            await _rewardRepository.UpdateAsync(reward);

            ScoreHistory scoreHistory = new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                CreatedAt = currentDateTime,
                Points = -reward.PointsCost,
                Origin = ScoreOrigin.Redemption,
                RelatedEntityId = reward.Id,
                StrategyName = "RedemptionStrategy"
            };

            await _scoreHistoryRepository.CreateAsync(scoreHistory);
            

            return MapToModelOut(redemption, visitor.Name, reward.Name);
        }

        public async Task<List<RedemptionHistoryModelOut>> GetRedemptionHistory(Guid visitorId)
        {
            List<RedemptionHistory> redemptions = await _redemptionHistoryRepository.GetByVisitorIdAsync(visitorId);
            return MapToModelOutList(redemptions);
        }

        public async Task<List<RedemptionHistoryModelOut>> GetRedemptionHistoryWithDateRange(Guid visitorId, DateTime dateFrom,
            DateTime dateTo)
        {
            List<RedemptionHistory> redemptions =
                await _redemptionHistoryRepository.GetByVisitorIdWithDateRangeAsync(visitorId, dateFrom, dateTo);
            return MapToModelOutList(redemptions);
        }

        private void ValidateRedemptionEligibility(User visitor, Reward reward)
        {
            HasSufficientPointsSpecification pointsSpec = new HasSufficientPointsSpecification(reward.PointsCost);
            MeetsRequiredMembershipSpecification membershipSpec =
                new MeetsRequiredMembershipSpecification(reward.RequiredMembershipLevel);
            RewardIsAvailableSpecification availabilitySpec = new RewardIsAvailableSpecification();

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

        private List<RedemptionHistoryModelOut> MapToModelOutList(List<RedemptionHistory> redemptions)
        {
            return redemptions.Select(r => new RedemptionHistoryModelOut
            {
                Id = r.Id,
                VisitorId = r.VisitorId,
                RewardId = r.RewardId,
                RedeemedAt = r.RedeemedAt,
                PointsSpent = r.PointsSpent,
                RewardName = r.Reward?.Name,
                VisitorName = r.Visitor?.Name
            }).ToList();
        }

        private RedemptionHistoryModelOut MapToModelOut(RedemptionHistory redemption, string? visitorName,
            string? rewardName)
        {
            return new RedemptionHistoryModelOut
            {
                Id = redemption.Id,
                VisitorId = redemption.VisitorId,
                RewardId = redemption.RewardId,
                RedeemedAt = redemption.RedeemedAt,
                PointsSpent = redemption.PointsSpent,
                RewardName = rewardName,
                VisitorName = visitorName
            };
        }
    }
}