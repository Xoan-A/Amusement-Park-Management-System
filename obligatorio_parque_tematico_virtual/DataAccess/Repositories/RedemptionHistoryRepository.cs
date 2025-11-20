using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class RedemptionHistoryRepository : IRedemptionHistoryRepository
    {
        private readonly AppDbContext _context;

        public RedemptionHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(RedemptionHistory redemptionHistory)
        {
            _context.RedemptionHistories.Add(redemptionHistory);
            _context.SaveChanges();
        }

        public List<RedemptionHistory> GetByVisitorId(Guid visitorId)
        {
            return _context.RedemptionHistories
            .Include(rh => rh.Visitor)
            .Include(rh => rh.Reward)
            .Where(rh => rh.VisitorId == visitorId)
            .OrderByDescending(rh => rh.RedeemedAt)
            .ToList();
        }

        public List<RedemptionHistory> GetByVisitorIdWithDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo)
        {
            return _context.RedemptionHistories
            .Include(rh => rh.Visitor)
            .Include(rh => rh.Reward)
            .Where(rh => rh.VisitorId == visitorId && rh.RedeemedAt >= dateFrom && rh.RedeemedAt <= dateTo)
            .OrderByDescending(rh => rh.RedeemedAt)
            .ToList();
        }
    }
}