using Domain;

namespace IBusinessLogic
{
    public interface IDailyScoreLogic
    {
        Task AddScoreToUser(User user, Attraction attraction, Event? attractionEvent = null);
    }
}

