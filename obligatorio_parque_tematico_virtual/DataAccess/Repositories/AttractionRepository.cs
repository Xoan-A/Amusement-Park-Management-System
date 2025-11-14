using DataAccess.Context;
using Domain;
using IDataAccess;

namespace DataAccess.Repositories;

public class AttractionRepository : IAttractionRepository
{
    private readonly AppDbContext _context;

    public AttractionRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Create(Attraction attraction)
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();
    }

    public Attraction GetByName(string name)
    {
        return _context.Attractions.FirstOrDefault(a => a.Name == name);
    }

    public Attraction GetById(Guid id)
    {
        return _context.Attractions.FirstOrDefault(a => a.Id == id);
    }

    public bool IsNameUnique(string name)
    {
        return !_context.Attractions.Any(a => a.Name == name);
    }

    public List<Attraction> GetAll()
    {
        return _context.Attractions.ToList();
    }

    public void Update(Attraction attraction)
    {
        _context.Attractions.Update(attraction);
        _context.SaveChanges();
    }

    public void Delete(Attraction attraction)
    {
        List<MaintenanceSchedule> maintenanceSchedules = _context.MaintenanceSchedules
        .Where(ms => ms.AttractionId == attraction.Id)
        .ToList();
        _context.MaintenanceSchedules.RemoveRange(maintenanceSchedules);

        List<EventAttraction> eventAttractions = _context.Set<EventAttraction>()
        .Where(ea => ea.AttractionId == attraction.Id)
        .ToList();
        _context.Set<EventAttraction>().RemoveRange(eventAttractions);

        _context.Attractions.Remove(attraction);
        _context.SaveChanges();
    }
}