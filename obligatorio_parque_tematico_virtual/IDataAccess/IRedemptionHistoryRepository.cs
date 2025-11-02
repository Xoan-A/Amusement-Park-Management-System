using Domain;

namespace IDataAccess
{
    public interface IRedemptionHistoryRepository
    {
        void Create(RedemptionHistory redemptionHistory);
        List<RedemptionHistory> GetByVisitorId(Guid visitorId);
        List<RedemptionHistory> GetByVisitorIdWithDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo);
        List<RedemptionHistory> GetAll();
    }
}
