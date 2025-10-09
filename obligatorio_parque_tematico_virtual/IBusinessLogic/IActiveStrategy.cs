using IBusinessLogic.Strategy;
using BusinessLogic;
using Domain;
using Models.In;

namespace IBusinessLogic;

public interface IActiveStrategy
{
    void SetStrategy(SetStrategyRequest setStrategyRequest);
    IContreteStrategy GetStrategy();
    Task<int> CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest);
}