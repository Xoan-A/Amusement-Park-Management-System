using Domain;

namespace IDataAccess
{
    public interface IRedemptionHistoryRepository
    {
        Task CreateAsync(RedemptionHistory redemptionHistory);
        Task<List<RedemptionHistory>> GetByVisitorIdAsync(Guid visitorId);
        Task<List<RedemptionHistory>> GetByVisitorIdWithDateRangeAsync(Guid visitorId, DateTime dateFrom, DateTime dateTo);
    }
}
