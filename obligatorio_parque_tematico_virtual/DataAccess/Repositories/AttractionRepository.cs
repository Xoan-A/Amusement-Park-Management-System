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
    
    public Attraction Create(Attraction attraction)
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();
        return attraction;
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
}