using Domain;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using IDataAccess;

namespace BusinessLogic
{
    public class DailyScoreLogic : IDateObserver, IDailyScoreLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IActiveStrategy _activeStrategy;
        private readonly IScoreHistoryRepository _scoreHistoryRepository;

        public DailyScoreLogic(IUserRepository userRepository, IActiveStrategy activeStrategy,
            IScoreHistoryRepository scoreHistoryRepository)
        {
            _userRepository = userRepository;
            _activeStrategy = activeStrategy;
            _scoreHistoryRepository = scoreHistoryRepository;
        }

        public void DateUpdated(IDateSubject subject)
        {
            DateTime previousDate = subject.GetPreviousDateTime().Date;
            DateTime currentDate = subject.GetCurrentDateTime().Date;

            if (previousDate != currentDate)
            {
                _userRepository.ResetScores();
            }
        }

        public void AddScoreToUser(User user, Attraction attraction, DateTime currentDateTime,
            Event? attractionEvent = null)
        {
            StrategyRequest strategyRequest = new StrategyRequest
            {
                UserId = user.Id,
                AttractionId = attraction.Id,
                IsSpecialEvent = attractionEvent != null,
            };

            int score = _activeStrategy.CalculateScore(user, attraction, strategyRequest);

            user.Score += score;
            user.DailyScore += score;
            _userRepository.Update(user);

            IConcreteStrategy currentStrategy = _activeStrategy.GetStrategy();

            ScoreHistory scoreHistory = new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = user.Id,
                CreatedAt = currentDateTime,
                Points = score,
                Origin = attractionEvent != null ? ScoreOrigin.EventParticipation : ScoreOrigin.AttractionVisit,
                RelatedEntityId = attractionEvent?.Id ?? attraction.Id,
                StrategyName = currentStrategy.Name,
            };

            _scoreHistoryRepository.Create(scoreHistory);
        }
    }
}