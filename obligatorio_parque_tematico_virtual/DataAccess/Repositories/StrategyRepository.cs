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

    public StrategyConfiguration? Get()
    {
        return _context.StrategyConfigurations.FirstOrDefault(s => s.Id == 1);
    }

    public void Update(StrategyConfiguration strategyConfiguration)
    {
        StrategyConfiguration? existing = _context.StrategyConfigurations.FirstOrDefault(s => s.Id == 1);

        if (existing != null)
        {
            existing.StrategyName = strategyConfiguration.StrategyName;
            existing.N = strategyConfiguration.N;
        }
        else
        {
            strategyConfiguration.Id = 1;
            _context.StrategyConfigurations.Add(strategyConfiguration);
        }

        _context.SaveChanges();
    }
}