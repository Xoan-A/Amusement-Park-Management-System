using Domain;
using IBusinessLogic.Strategy;
using Models.In;

namespace BusinessLogic;

public class PerEvent : IConcreteStrategy
{
    public string Name => "PerEvent";

    public int CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {
        int score = ActiveStrategy.BasicCalculation(visitor, attraction);

        if (strategyRequest.IsSpecialEvent)
            score *= 2;

        return score;
    }
}