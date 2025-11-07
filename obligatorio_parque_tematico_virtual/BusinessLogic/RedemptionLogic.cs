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

        public async Task<List<RedemptionHistoryModelOut>> GetRedemptionHistoryWithDateRange(Guid visitorId,
            DateTime dateFrom,
            DateTime dateTo)
        {
            List<RedemptionHistory> redemptions =
            await _redemptionHistoryRepository.GetByVisitorIdWithDateRangeAsync(visitorId, dateFrom, dateTo);
            return MapToModelOutList(redemptions);
        }

        private void ValidateRedemptionEligibility(User visitor, Reward reward)
        {
            ValidateHasSufficientPoints(visitor, reward.PointsCost);
            ValidateMeetsRequiredMembership(visitor, reward.RequiredMembershipLevel);
            ValidateRewardIsAvailable(reward);
        }

        private void ValidateHasSufficientPoints(User visitor, int requiredPoints)
        {
            if (visitor.Score < requiredPoints)
            {
                throw new InvalidOperationException(
                    $"Visitor does not have sufficient points. Required: {requiredPoints}, Available: {visitor.Score}");
            }
        }

        private void ValidateMeetsRequiredMembership(User visitor, MembershipLevel? requiredLevel)
        {
            if (requiredLevel == null)
            {
                return;
            }

            if (visitor.MembershipLevel == null)
            {
                throw new InvalidOperationException(
                    $"Visitor does not meet the required membership level. Required: {requiredLevel}, Current: None");
            }

            if (visitor.MembershipLevel < requiredLevel)
            {
                throw new InvalidOperationException(
                    $"Visitor does not meet the required membership level. Required: {requiredLevel}, Current: {visitor.MembershipLevel}");
            }
        }

        private void ValidateRewardIsAvailable(Reward reward)
        {
            if (!reward.IsAvailable())
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