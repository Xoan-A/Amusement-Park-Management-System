using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class DailyScoreLogic : IDateObserver, IDailyScoreLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IActiveStrategy _activeStrategy;

        public DailyScoreLogic(IUserRepository userRepository, IActiveStrategy activeStrategy)
        {
            _userRepository = userRepository;
            _activeStrategy = activeStrategy;
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

        public async Task AddScoreToUser(User user, Attraction attraction, bool isSpecialEvent)
        {
            StrategyRequest strategyRequest = new StrategyRequest
            {
                UserId = user.Id,
                AttractionId = attraction.Id,
                IsSepcialEvent = isSpecialEvent,
            };

            int score = _activeStrategy.CalculateScore(user, attraction, strategyRequest);

            user.Score += score;
            await _userRepository.Update(user);
        }
    }
}
