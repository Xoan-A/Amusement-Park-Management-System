using Domain;
using IBusinessLogic.Strategy;

namespace BusinessLogic;

public class PerEvent : IConcreteStrategy
{
    public string Name => "PerEvent";

    public int CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {
        int score = ActiveStrategy.BasicCalculation(visitor, attraction);

        if (strategyRequest.IsSepcialEvent)
            score *= 2;

        return score;
    }
}