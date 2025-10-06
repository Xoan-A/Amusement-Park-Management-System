using IBusinessLogic.Strategy;
using BusinessLogic;
using Models.In;

namespace IBusinessLogic;

public interface IActiveStrategy
{
    void SetStrategy(SetStrategyRequest setStrategyRequest);
    IContreteStrategy GetStrategy(DateTime currentDate);
    int CalculateScore(StrategyRequest strategyRequest);
}