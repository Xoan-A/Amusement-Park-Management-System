using IBusinessLogic.Strategy;
using BusinessLogic;
using Domain;
using Models.In;

namespace IBusinessLogic;

public interface IActiveStrategy
{
    void SetStrategy(SetStrategyRequest setStrategyRequest);
    IConcreteStrategy GetStrategy();
    int CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest);
}