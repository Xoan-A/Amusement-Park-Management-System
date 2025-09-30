using DataAccess.Context;
using Domain;
using IDataAccess;

namespace DataAccess.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;
    public EventRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Event> GetById(Guid id)
    {
        return await _context.Events.FindAsync(id);
    }

    public async Task Create(Event eventEntity)
    {
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();
    }
}