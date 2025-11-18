using IBusinessLogic;
using Domain;
using Models.In;

namespace BusinessLogic;

public class PerAttraction : IConcreteStrategy
{
    public string Name => "PerAttraction";

    public int CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {
        return ActiveStrategy.BasicCalculation(visitor, attraction);
    }
}