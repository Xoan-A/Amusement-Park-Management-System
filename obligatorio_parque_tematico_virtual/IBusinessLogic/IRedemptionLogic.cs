using Models.Out;

namespace IBusinessLogic
{
    public interface IRedemptionLogic
    {
        RedemptionHistoryModelOut RedeemReward(Guid visitorId, Guid rewardId);
        List<RedemptionHistoryModelOut> GetRedemptionHistory(Guid visitorId);
        List<RedemptionHistoryModelOut> GetRedemptionHistoryWithDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo);
    }
}
