using Domain;

namespace IDataAccess
{
    public interface IScoreHistoryRepository
    {
        Task CreateAsync(ScoreHistory history);
        Task<List<ScoreHistory>> GetByVisitorAsync(Guid visitorId);
        Task<List<ScoreHistory>> GetByVisitorAndDateRangeAsync(Guid visitorId, DateTime dateFrom, DateTime dateTo);
        Task<List<ScoreHistory>> GetAllAsync();
    }
}
