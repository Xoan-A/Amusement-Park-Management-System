using Domain;

namespace IDataAccess;

public interface IEventRepository
{
    public Task<Event> GetById(Guid id);
    Task Create(Event eventEntity);
    Task<List<Event>> GetAll();
    Task Delete(Event eventEntity);
    Task<Event?> GetEventByAttractionAndDate(Guid attractionId, DateTime date);
}