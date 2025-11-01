using Domain;

namespace IDataAccess
{
    public interface IScoreHistoryRepository
    {
        void Create(ScoreHistory history);
        ScoreHistory? GetById(Guid id);
        List<ScoreHistory> GetByVisitor(Guid visitorId);
        List<ScoreHistory> GetByVisitorAndDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo);
        List<ScoreHistory> GetByOrigin(ScoreOrigin origin);
        List<ScoreHistory> GetAll();
    }
}
