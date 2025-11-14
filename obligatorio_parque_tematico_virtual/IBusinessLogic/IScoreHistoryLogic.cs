using Models.Out;

namespace IBusinessLogic;

public interface IScoreHistoryLogic
{
    List<ScoreHistoryModelOut> GetMyScoreHistory(Guid visitorId);
    List<ScoreHistoryModelOut> GetVisitorScoreHistory(Guid visitorId, DateTime? dateFrom, DateTime? dateTo);
    List<ScoreHistoryModelOut> GetAllScoreHistory();
}
