using Domain;

namespace IBusinessLogic
{
    public interface IDailyScoreLogic
    {
        void AddScoreToUser(User user, Attraction attraction, DateTime currentDateTime, Event? attractionEvent = null);
    }
}

