using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Event>> GetAll()
    {
        return await _context.Events.ToListAsync();
    }

    public async Task Delete(Event eventEntity)
    {
        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
    }

    public async Task<Event?> GetEventByAttractionAndDate(Guid attractionId, DateTime date)
    {
        return await _context.Events
            .Include(e => e.Attractions)
            .FirstOrDefaultAsync(e =>
                e.Date.Date == date.Date &&
                e.Attractions.Any(ea => ea.AttractionId == attractionId));
    }
}