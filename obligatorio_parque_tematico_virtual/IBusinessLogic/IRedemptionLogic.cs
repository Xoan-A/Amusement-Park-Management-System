using Models.Out;

namespace IBusinessLogic
{
    public interface IRedemptionLogic
    {
        Task<RedemptionHistoryModelOut> RedeemReward(Guid visitorId, Guid rewardId);
        Task<List<RedemptionHistoryModelOut>> GetRedemptionHistory(Guid visitorId);
        Task<List<RedemptionHistoryModelOut>> GetRedemptionHistoryWithDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo);
    }
}
