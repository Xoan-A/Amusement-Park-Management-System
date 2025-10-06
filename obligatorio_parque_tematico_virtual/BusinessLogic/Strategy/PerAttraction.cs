using IBusinessLogic.Strategy;
using Domain;

namespace BusinessLogic;

public class PerAttraction : IContreteStrategy
{
    public string Name => "PerAttraction";
    public int CalculateScore(StrategyRequest strategyRequest)
    {
        return ActiveStrategy.BasicCalculation(strategyRequest);
    }
}