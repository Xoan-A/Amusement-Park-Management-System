using Models.Out;

namespace IBusinessLogic;

public interface IEventService
{
    Task<EventResponse> GetEventById(Guid expectedEventId);
    Task<List<EventResponse>> GetAllEvents();
}