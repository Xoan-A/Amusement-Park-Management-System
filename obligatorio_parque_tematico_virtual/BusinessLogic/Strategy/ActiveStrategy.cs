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
    private readonly IPluginLoader _pluginLoader;

    public ActiveStrategy(IStrategyRepository strategyRepository, IPluginLoader pluginLoader)
    {
        _strategyRepository = strategyRepository;
        _pluginLoader = pluginLoader;
        LoadStrategyFromDatabase();
    }

    private void LoadStrategyFromDatabase()
    {
        StrategyConfiguration? config = _strategyRepository.Get().GetAwaiter().GetResult();
        if (config != null)
        {
            try
            {
                Dictionary<string, object>? parameters = null;
                if (config.StrategyName == "Combo")
                {
                    if (config.N == null)
                        throw new ArgumentException("N is required for Combo strategy");
                    parameters = new Dictionary<string, object> { ["n"] = config.N.Value };
                }

                Strategy = _pluginLoader.CreateStrategyInstance(config.StrategyName, parameters);
            }
            catch (KeyNotFoundException)
            {
                Strategy = _pluginLoader.CreateStrategyInstance("PerAttraction");
            }
        }
        else
        {
            Strategy = _pluginLoader.CreateStrategyInstance("PerAttraction");
        }
    }

    public async Task SetStrategy(SetStrategyRequest setStrategyRequest)
    {
        Dictionary<string, object>? parameters = null;
        if (setStrategyRequest.StrategyName == "Combo")
        {
            if (setStrategyRequest.N == null)
                throw new ArgumentException("N is required for Combo strategy");
            parameters = new Dictionary<string, object> { ["n"] = setStrategyRequest.N.Value };
        }

        try
        {
            IConcreteStrategy strategy = _pluginLoader.CreateStrategyInstance(setStrategyRequest.StrategyName, parameters);
            Strategy = strategy;
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"Invalid strategy name: {setStrategyRequest.StrategyName}");
        }

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