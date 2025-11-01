using Domain;

namespace IBusinessLogic
{
    public interface IRedemptionLogic
    {
        RedemptionHistory RedeemReward(Guid visitorId, Guid rewardId);
        List<RedemptionHistory> GetRedemptionHistory(Guid visitorId);
        List<RedemptionHistory> GetRedemptionHistoryWithDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo);
    }
}
