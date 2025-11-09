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

        public DailyScoreLogic(IUserRepository userRepository, IActiveStrategy activeStrategy, IScoreHistoryRepository scoreHistoryRepository)
        {
            _userRepository = userRepository;
            _activeStrategy = activeStrategy;
            _scoreHistoryRepository = scoreHistoryRepository;
        }

        public async Task DateUpdated(IDateSubject subject)
        {
            var previousDate = subject.GetPreviousDateTime().Date;
            var currentDate = (await subject.GetCurrentDateTime()).Date;

            if (previousDate != currentDate)
            {
                await _userRepository.ResetScores();
            }
        }

        public async Task AddScoreToUser(User user, Attraction attraction, Event? attractionEvent = null)
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
            await _userRepository.Update(user);
            
            IConcreteStrategy currentStrategy = await _activeStrategy.GetStrategy();

            var scoreHistory = new ScoreHistory
            {
                Id = Guid.NewGuid(),
                VisitorId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Points = score,
                Origin = attractionEvent != null ? ScoreOrigin.EventParticipation : ScoreOrigin.AttractionVisit,
                RelatedEntityId = attractionEvent?.Id ?? attraction.Id,
                StrategyName = currentStrategy.Name,
            };

            await _scoreHistoryRepository.CreateAsync(scoreHistory);
        }
    }
}
