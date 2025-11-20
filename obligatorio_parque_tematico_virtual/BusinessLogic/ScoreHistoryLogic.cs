using AutoMapper;
using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.Out;

namespace BusinessLogic
{
    public class ScoreHistoryLogic : IScoreHistoryLogic
    {
        private readonly IScoreHistoryRepository _scoreHistoryRepository;
        private readonly IMapper _mapper;

        public ScoreHistoryLogic(IScoreHistoryRepository scoreHistoryRepository, IMapper mapper)
        {
            _scoreHistoryRepository = scoreHistoryRepository;
            _mapper = mapper;
        }

        public List<ScoreHistoryModelOut> GetMyScoreHistory(Guid visitorId)
        {
            List<ScoreHistory> histories = _scoreHistoryRepository.GetByVisitor(visitorId);
            return MapToModelOut(histories);
        }

        public List<ScoreHistoryModelOut> GetVisitorScoreHistory(Guid visitorId, DateTime? dateFrom, DateTime? dateTo)
        {
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                List<ScoreHistory> histories =
                _scoreHistoryRepository.GetByVisitorAndDateRange(visitorId, dateFrom.Value, dateTo.Value);
                return MapToModelOut(histories);
            }

            List<ScoreHistory> allHistories = _scoreHistoryRepository.GetByVisitor(visitorId);
            return MapToModelOut(allHistories);
        }

        public List<ScoreHistoryModelOut> GetAllScoreHistory()
        {
            List<ScoreHistory> histories = _scoreHistoryRepository.GetAll();
            return MapToModelOut(histories);
        }

        private List<ScoreHistoryModelOut> MapToModelOut(List<ScoreHistory> histories)
        {
            return _mapper.Map<List<ScoreHistoryModelOut>>(histories);
        }
    }
}