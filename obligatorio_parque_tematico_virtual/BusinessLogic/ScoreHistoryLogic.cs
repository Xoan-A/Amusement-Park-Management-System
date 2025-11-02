using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.Out;

namespace BusinessLogic
{
    public class ScoreHistoryLogic : IScoreHistoryLogic
    {
        private readonly IScoreHistoryRepository _scoreHistoryRepository;

        public ScoreHistoryLogic(IScoreHistoryRepository scoreHistoryRepository)
        {
            _scoreHistoryRepository = scoreHistoryRepository;
        }

        public List<ScoreHistoryModelOut> GetMyScoreHistory(Guid visitorId)
        {
            var histories = _scoreHistoryRepository.GetByVisitor(visitorId);
            return MapToModelOut(histories);
        }

        public List<ScoreHistoryModelOut> GetVisitorScoreHistory(Guid visitorId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                var histories = _scoreHistoryRepository.GetByVisitorAndDateRange(visitorId, dateFrom.Value, dateTo.Value);
                return MapToModelOut(histories);
            }

            var allHistories = _scoreHistoryRepository.GetByVisitor(visitorId);
            return MapToModelOut(allHistories);
        }

        public List<ScoreHistoryModelOut> GetAllScoreHistory()
        {
            var histories = _scoreHistoryRepository.GetAll();
            return MapToModelOut(histories);
        }

        private List<ScoreHistoryModelOut> MapToModelOut(List<ScoreHistory> histories)
        {
            return histories.Select(h => new ScoreHistoryModelOut
            {
                Id = h.Id,
                VisitorId = h.VisitorId,
                VisitorName = h.Visitor?.Name,
                Points = h.Points,
                Origin = h.Origin.ToString(),
                Description = h.Description,
                StrategyName = h.StrategyName,
                RelatedEntityId = h.RelatedEntityId,
                CreatedAt = h.CreatedAt
            }).ToList();
        }
    }
}
