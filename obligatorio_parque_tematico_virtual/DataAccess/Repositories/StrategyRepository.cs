using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class StrategyRepository : IStrategyRepository
{
    private readonly AppDbContext _context;

    public StrategyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StrategyConfiguration?> Get()
    {
        return await _context.StrategyConfigurations.FirstOrDefaultAsync(s => s.Id == 1);
    }

    public async Task Update(StrategyConfiguration strategyConfiguration)
    {
        var existing = await _context.StrategyConfigurations.FirstOrDefaultAsync(s => s.Id == 1);

        if (existing != null)
        {
            existing.StrategyName = strategyConfiguration.StrategyName;
            existing.N = strategyConfiguration.N;
        }
        else
        {
            strategyConfiguration.Id = 1;
            await _context.StrategyConfigurations.AddAsync(strategyConfiguration);
        }

        await _context.SaveChangesAsync();
    }
}
