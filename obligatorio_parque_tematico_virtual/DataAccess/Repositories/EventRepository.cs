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

    public Event GetById(Guid id)
    {
        return _context.Events
        .Include(e => e.Attractions)
        .ThenInclude(ea => ea.Attraction)
        .FirstOrDefault(e => e.Id == id);
    }

    public void Create(Event eventEntity)
    {
        _context.Events.Add(eventEntity);
        _context.SaveChanges();
    }

    public List<Event> GetAll()
    {
        return _context.Events
        .Include(e => e.Attractions)
        .ThenInclude(ea => ea.Attraction)
        .ToList();
    }

    public void Delete(Event eventEntity)
    {
        _context.Events.Remove(eventEntity);
        _context.SaveChanges();
    }

    public Event? GetEventByAttractionAndDate(Guid attractionId, DateTime date)
    {
        return _context.Events
        .Include(e => e.Attractions)
        .ThenInclude(ea => ea.Attraction)
        .FirstOrDefault(e =>
        e.Date.Date == date.Date &&
        e.Attractions.Any(ea => ea.AttractionId == attractionId));
    }
}