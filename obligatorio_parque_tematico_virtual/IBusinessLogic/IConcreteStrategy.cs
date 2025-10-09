using BusinessLogic;
using Domain;

namespace IBusinessLogic.Strategy;

public interface IContreteStrategy
{
    public string Name { get; }
    public Task<int> CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest);
}