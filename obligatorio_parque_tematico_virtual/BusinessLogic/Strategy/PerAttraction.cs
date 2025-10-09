using IBusinessLogic.Strategy;
using Domain;

namespace BusinessLogic;

public class PerAttraction : IConcreteStrategy
{
    public string Name => "PerAttraction";

    public int CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {
        return ActiveStrategy.BasicCalculation(visitor, attraction);
    }
}