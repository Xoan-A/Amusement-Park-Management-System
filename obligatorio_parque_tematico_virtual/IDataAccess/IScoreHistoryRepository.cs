using Domain;

namespace IDataAccess
{
    public interface IScoreHistoryRepository
    {
        void Create(ScoreHistory history);
        List<ScoreHistory> GetByVisitor(Guid visitorId);
        List<ScoreHistory> GetByVisitorAndDateRange(Guid visitorId, DateTime dateFrom, DateTime dateTo);
        List<ScoreHistory> GetAll();
    }
}
