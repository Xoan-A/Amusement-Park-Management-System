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

        public async Task<List<ScoreHistoryModelOut>> GetMyScoreHistory(Guid visitorId)
        {
            List<ScoreHistory> histories = await _scoreHistoryRepository.GetByVisitorAsync(visitorId);
            return MapToModelOut(histories);
        }

        public async Task<List<ScoreHistoryModelOut>> GetVisitorScoreHistory(Guid visitorId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                List<ScoreHistory> histories =
                    await _scoreHistoryRepository.GetByVisitorAndDateRangeAsync(visitorId, dateFrom.Value, dateTo.Value);
                return MapToModelOut(histories);
            }

            List<ScoreHistory> allHistories = await _scoreHistoryRepository.GetByVisitorAsync(visitorId);
            return MapToModelOut(allHistories);
        }

        public async Task<List<ScoreHistoryModelOut>> GetAllScoreHistory()
        {
            List<ScoreHistory> histories = await _scoreHistoryRepository.GetAllAsync();
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
                StrategyName = h.StrategyName,
                RelatedEntityId = h.RelatedEntityId,
                CreatedAt = h.CreatedAt
            }).ToList();
        }
    }
}