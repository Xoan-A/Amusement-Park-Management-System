using AutoMapper;
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
        private readonly IMapper _mapper;

        public RedemptionLogic(
            IRewardRepository rewardRepository,
            IUserRepository userRepository,
            IRedemptionHistoryRepository redemptionHistoryRepository,
            IDateTimeLogic dateTimeLogic,
            IScoreHistoryRepository scoreHistoryRepository,
            IMapper mapper)
        {
            _rewardRepository = rewardRepository;
            _userRepository = userRepository;
            _redemptionHistoryRepository = redemptionHistoryRepository;
            _dateTimeLogic = dateTimeLogic;
            _scoreHistoryRepository = scoreHistoryRepository;
            _mapper = mapper;
        }

        public RedemptionHistoryModelOut RedeemReward(Guid visitorId, Guid rewardId)
        {
            User? visitor = _userRepository.GetById(visitorId);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID '{visitorId}' not found");
            }

            Reward? reward = _rewardRepository.GetById(rewardId);
            if (reward == null)
            {
                throw new KeyNotFoundException($"Reward with ID '{rewardId}' not found");
            }

            ValidateRedemptionEligibility(visitor, reward);

            visitor.Score -= reward.PointsCost;
            reward.DecrementQuantity();

            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();

            RedemptionHistory redemption = new RedemptionHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                RewardId = reward.Id,
                RedeemedAt = currentDateTime,
                PointsSpent = reward.PointsCost
            };

            _redemptionHistoryRepository.Create(redemption);
            _userRepository.Update(visitor);
            _rewardRepository.Update(reward);

            ScoreHistory scoreHistory = new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = visitor.Id,
                CreatedAt = currentDateTime,
                Points = -reward.PointsCost,
                Origin = ScoreOrigin.Redemption,
                RelatedEntityId = reward.Id,
                RelatedEntityName = reward.Name,
                StrategyName = "RedemptionStrategy"
            };

            _scoreHistoryRepository.Create(scoreHistory);

            return MapToModelOut(redemption, visitor.Name, reward.Name);
        }

        public List<RedemptionHistoryModelOut> GetRedemptionHistory(Guid visitorId)
        {
            List<RedemptionHistory> redemptions = _redemptionHistoryRepository.GetByVisitorId(visitorId);
            return MapToModelOutList(redemptions);
        }

        public List<RedemptionHistoryModelOut> GetRedemptionHistoryWithDateRange(Guid visitorId,
            DateTime dateFrom,
            DateTime dateTo)
        {
            List<RedemptionHistory> redemptions =
            _redemptionHistoryRepository.GetByVisitorIdWithDateRange(visitorId, dateFrom, dateTo);
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
            return _mapper.Map<List<RedemptionHistoryModelOut>>(redemptions);
        }

        private RedemptionHistoryModelOut MapToModelOut(RedemptionHistory redemption, string? visitorName,
            string? rewardName)
        {
            RedemptionHistoryModelOut mapped = _mapper.Map<RedemptionHistoryModelOut>(redemption);
            if (visitorName != null)
                mapped.VisitorName = visitorName;
            if (rewardName != null)
                mapped.RewardName = rewardName;
            return mapped;
        }
    }
}