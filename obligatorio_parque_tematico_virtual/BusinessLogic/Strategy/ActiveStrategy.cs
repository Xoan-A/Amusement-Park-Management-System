using Domain;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using Models.In;

namespace BusinessLogic;

public class ActiveStrategy : IActiveStrategy
{
    private IContreteStrategy Strategy;
    public DateTime LastUpdated { get; set; }


    public void SetStrategy(SetStrategyRequest setStrategyRequest)
    {
        IContreteStrategy strategy = setStrategyRequest.StrategyName switch
        {
            "PerAttraction" => new PerAttraction(),
            "PerEvent" => new PerEvent(),
            "Combo" => new Combo(
                setStrategyRequest.N ?? throw new ArgumentException("N is required for Combo strategy")),
            _ => throw new ArgumentException($"Invalid strategy name: {setStrategyRequest.StrategyName}")
        };
        
        Strategy = strategy;
        LastUpdated = setStrategyRequest.CurrentDate;
    }

    public IContreteStrategy GetStrategy(DateTime currentDate)
    {
        if (Strategy == null)
            throw new InvalidOperationException("Strategy not set");

        return Strategy;
    }

    public static int BasicCalculation(StrategyRequest strategyRequest)
    {
        User visitor = strategyRequest.User;
        Attraction attraction = strategyRequest.Attraction;

        if (visitor == null || attraction == null)
            throw new ArgumentException("Visitor and Attraction cannot be null");

        int score = 0;
        switch (attraction.Type)
        {
            case AttractionType.RollerCoaster:
                score = 2;
                break;
            case AttractionType.Simulator:
                score = 2;
                break;
            case AttractionType.Performance:
                score = 3;
                break;
            case AttractionType.InteractiveZone:
                score = 4;
                break;
        }

        return score;
    }

    public int CalculateScore(StrategyRequest strategyRequest)
    {
        return Strategy.CalculateScore(strategyRequest);
    }
}