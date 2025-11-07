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

        public async Task CreateAsync(RedemptionHistory redemptionHistory)
        {
            _context.RedemptionHistories.Add(redemptionHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RedemptionHistory>> GetByVisitorIdAsync(Guid visitorId)
        {
            return await _context.RedemptionHistories
                .Include(rh => rh.Visitor)
                .Include(rh => rh.Reward)
                .Where(rh => rh.VisitorId == visitorId)
                .OrderByDescending(rh => rh.RedeemedAt)
                .ToListAsync();
        }

        public async Task<List<RedemptionHistory>> GetByVisitorIdWithDateRangeAsync(Guid visitorId, DateTime dateFrom, DateTime dateTo)
        {
            return await _context.RedemptionHistories
                .Include(rh => rh.Visitor)
                .Include(rh => rh.Reward)
                .Where(rh => rh.VisitorId == visitorId && rh.RedeemedAt >= dateFrom && rh.RedeemedAt <= dateTo)
                .OrderByDescending(rh => rh.RedeemedAt)
                .ToListAsync();
        }

        public async Task<List<RedemptionHistory>> GetAllAsync()
        {
            return await _context.RedemptionHistories
                .Include(rh => rh.Visitor)
                .Include(rh => rh.Reward)
                .OrderByDescending(rh => rh.RedeemedAt)
                .ToListAsync();
        }
    }
}
