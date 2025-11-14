using Models.Out;
using Models.In;

namespace IBusinessLogic;

public interface IEventLogic
{
    EventResponse GetEventById(Guid expectedEventId);
    List<EventResponse> GetAllEvents();
    Guid CreateEvent(EventRequest newEvent);
    void DeleteEvent(Guid eventId);
}