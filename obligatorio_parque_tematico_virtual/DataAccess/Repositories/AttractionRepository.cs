using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class AttractionRepository : IAttractionRepository
{
    private readonly AppDbContext _context;

    public AttractionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task Create(Attraction attraction)
    {
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();
    }

    public async Task<Attraction> GetByName(string name)
    {
        return await _context.Attractions.FirstOrDefaultAsync(a => a.Name == name);
    }

    public async Task<Attraction> GetById(Guid id)
    {
        return await _context.Attractions.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> IsNameUnique(string name)
    {
        return !await _context.Attractions.AnyAsync(a => a.Name == name);
    }

    public async Task<List<Attraction>> GetAll()
    {
        return await _context.Attractions.ToListAsync();
    }

    public async Task Update(Attraction attraction)
    {
        _context.Attractions.Update(attraction);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Attraction attraction)
    {
        _context.Attractions.Remove(attraction);
        await _context.SaveChangesAsync();
    }
}