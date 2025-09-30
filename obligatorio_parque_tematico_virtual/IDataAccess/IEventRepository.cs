using Domain;

namespace IDataAccess;

public interface IEventRepository
{
    public Task<Event> GetById(Guid id);
}