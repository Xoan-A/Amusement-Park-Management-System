using Domain;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using IDataAccess;
using Microsoft.Extensions.DependencyInjection;
using Models.In;

namespace BusinessLogic;

public class ActiveStrategy : IActiveStrategy
{
    private IContreteStrategy Strategy;
    private readonly IStrategyRepository _strategyRepository;

    public ActiveStrategy(IStrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
        LoadStrategyFromDatabase();
    }

    private void LoadStrategyFromDatabase()
    {
        var config = _strategyRepository.Get();
        if (config != null)
        {
            Strategy = config.StrategyName switch
            {
                "PerAttraction" => new PerAttraction(),
                "PerEvent" => new PerEvent(),
                "Combo" => new Combo(config.N ?? throw new ArgumentException("N is required for Combo strategy")),
                _ => new PerAttraction()
            };
        }
        else
        {
            Strategy = new PerAttraction();
        }
    }

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

        var config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = setStrategyRequest.StrategyName,
            N = setStrategyRequest.N,
        };
        _strategyRepository.Update(config);
    }

    public IContreteStrategy GetStrategy()
    {
        LoadStrategyFromDatabase();
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