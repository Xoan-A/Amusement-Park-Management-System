using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ScoreHistoryRepository : IScoreHistoryRepository
    {
        private readonly AppDbContext _context;

        public ScoreHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(ScoreHistory history)
        {
            _context.ScoreHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ScoreHistory>> GetByVisitorAsync(Guid visitorId)
        {
            return await _context.ScoreHistories
                .Include(h => h.Visitor)
                .Where(h => h.VisitorId == visitorId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ScoreHistory>> GetByVisitorAndDateRangeAsync(Guid visitorId, DateTime dateFrom, DateTime dateTo)
        {
            return await _context.ScoreHistories
                .Include(h => h.Visitor)
                .Where(h => h.VisitorId == visitorId &&
                           h.CreatedAt >= dateFrom &&
                           h.CreatedAt <= dateTo)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ScoreHistory>> GetAllAsync()
        {
            return await _context.ScoreHistories
                .Include(h => h.Visitor)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}
