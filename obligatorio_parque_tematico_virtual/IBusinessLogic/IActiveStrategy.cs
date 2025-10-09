using IBusinessLogic.Strategy;
using BusinessLogic;
using Domain;
using Models.In;

namespace IBusinessLogic;

public interface IActiveStrategy
{
    Task SetStrategy(SetStrategyRequest setStrategyRequest);
    Task<IConcreteStrategy> GetStrategy();
    int CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest);
}