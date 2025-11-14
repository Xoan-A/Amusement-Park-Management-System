using Domain;

namespace IDataAccess;

public interface IStrategyRepository
{
    StrategyConfiguration? Get();
    void Update(StrategyConfiguration strategyConfiguration);
}
