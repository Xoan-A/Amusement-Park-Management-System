using Domain;

namespace IDataAccess;

public interface IEventRepository
{
    public Event GetById(Guid id);
    void Create(Event eventEntity);
    List<Event> GetAll();
    void Delete(Event eventEntity);
    Event? GetEventByAttractionAndDate(Guid attractionId, DateTime date);
}