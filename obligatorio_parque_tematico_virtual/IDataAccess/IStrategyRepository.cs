using Domain;

namespace IDataAccess;

public interface IStrategyRepository
{
    Task<StrategyConfiguration?> Get();
    Task Update(StrategyConfiguration strategyConfiguration);
}
