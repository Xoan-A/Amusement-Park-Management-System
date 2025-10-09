using Domain;
using IBusinessLogic.Strategy;

namespace BusinessLogic;

public class PerEvent : IContreteStrategy
{
    public string Name => "PerEvent";

    public async Task<int> CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {
        int score = await ActiveStrategy.BasicCalculation(visitor, attraction);

        if (strategyRequest.IsSepcialEvent)
            score *= 2;

        return score;
    }
}