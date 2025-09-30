using Models.Out;
using Models.In;

namespace IBusinessLogic;

public interface IEventService
{
    Task<EventResponse> GetEventById(Guid expectedEventId);
    Task<List<EventResponse>> GetAllEvents();
    Task<Guid> CreateEvent(EventRequest newEvent);
}