using Domain;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using IDataAccess;
using Models.In;

namespace BusinessLogic;

public class ActiveStrategy : IActiveStrategy
{
    private IConcreteStrategy Strategy;
    private readonly IStrategyRepository _strategyRepository;

    public ActiveStrategy(IStrategyRepository strategyRepository)
    {
        _strategyRepository = strategyRepository;
        LoadStrategyFromDatabase();
    }

    private void LoadStrategyFromDatabase()
    {
        StrategyConfiguration? config = _strategyRepository.Get().GetAwaiter().GetResult();
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

    public async Task SetStrategy(SetStrategyRequest setStrategyRequest)
    {
        IConcreteStrategy strategy = setStrategyRequest.StrategyName switch
        {
            "PerAttraction" => new PerAttraction(),
            "PerEvent" => new PerEvent(),
            "Combo" => new Combo(
                setStrategyRequest.N ?? throw new ArgumentException("N is required for Combo strategy")),
            _ => throw new ArgumentException($"Invalid strategy name: {setStrategyRequest.StrategyName}")
        };

        Strategy = strategy;

        StrategyConfiguration config = new StrategyConfiguration
        {
            Id = 1,
            StrategyName = setStrategyRequest.StrategyName,
            N = setStrategyRequest.N,
        };
        await _strategyRepository.Update(config);
    }

    public Task<IConcreteStrategy> GetStrategy()
    {
        LoadStrategyFromDatabase();
        if (Strategy == null)
            throw new InvalidOperationException("Strategy not set");

        return Task.FromResult(Strategy);
    }

    public static int BasicCalculation(User visitor, Attraction attraction)
    {
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

    public int CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest)
    {
        int score = Strategy.CalculateScore(user, attraction, strategyRequest);
        return score;
    }
}