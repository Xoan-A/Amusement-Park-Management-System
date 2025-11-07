using Models.Out;

namespace IBusinessLogic;

public interface IScoreHistoryLogic
{
    Task<List<ScoreHistoryModelOut>> GetMyScoreHistory(Guid visitorId);
    Task<List<ScoreHistoryModelOut>> GetVisitorScoreHistory(Guid visitorId, DateTime? dateFrom, DateTime? dateTo);
    Task<List<ScoreHistoryModelOut>> GetAllScoreHistory();
}
