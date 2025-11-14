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

        public void Create(ScoreHistory history)
        {
            _context.ScoreHistories.Add(history);
            _context.SaveChanges();
        }

        public List<ScoreHistory> GetByVisitor(Guid visitorId)
        {
            return _context.ScoreHistories
            .Include(h => h.Visitor)
            .Where(h => h.VisitorId == visitorId)
            .OrderByDescending(h => h.CreatedAt)
            .ToList();
        }

        public List<ScoreHistory> GetByVisitorAndDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo)
        {
            return _context.ScoreHistories
            .Include(h => h.Visitor)
            .Where(h => h.VisitorId == visitorId &&
                        h.CreatedAt >= dateFrom &&
                        h.CreatedAt <= dateTo)
            .OrderByDescending(h => h.CreatedAt)
            .ToList();
        }

        public List<ScoreHistory> GetAll()
        {
            return _context.ScoreHistories
            .Include(h => h.Visitor)
            .OrderByDescending(h => h.CreatedAt)
            .ToList();
        }
    }
}