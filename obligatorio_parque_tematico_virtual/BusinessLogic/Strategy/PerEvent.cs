using Domain;
using IBusinessLogic.Strategy;

namespace BusinessLogic;

public class PerEvent : IContreteStrategy
{
    public string Name => "PerEvent";

    public int CalculateScore(StrategyRequest strategyRequest)
    {
        int score = ActiveStrategy.BasicCalculation(strategyRequest);

        if (strategyRequest.IsSepcialEvent)
            score *= 2;

        return score;
    }
}