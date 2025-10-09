using IBusinessLogic.Strategy;
using Domain;

namespace BusinessLogic;

public class PerAttraction : IContreteStrategy
{
    public string Name => "PerAttraction";

    public async Task<int> CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {
        return await ActiveStrategy.BasicCalculation(visitor, attraction);
    }
}