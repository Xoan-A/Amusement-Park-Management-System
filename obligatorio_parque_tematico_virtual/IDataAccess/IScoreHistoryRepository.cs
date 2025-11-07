using Domain;

namespace IDataAccess
{
    public interface IScoreHistoryRepository
    {
        Task CreateAsync(ScoreHistory history);
        Task<ScoreHistory?> GetByIdAsync(Guid id);
        Task<List<ScoreHistory>> GetByVisitorAsync(Guid visitorId);
        Task<List<ScoreHistory>> GetByVisitorAndDateRangeAsync(Guid visitorId, DateTime dateFrom, DateTime dateTo);
        Task<List<ScoreHistory>> GetByOriginAsync(ScoreOrigin origin);
        Task<List<ScoreHistory>> GetAllAsync();
    }
}
