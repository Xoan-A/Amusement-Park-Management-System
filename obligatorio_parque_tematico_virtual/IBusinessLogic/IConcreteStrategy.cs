using Models.In;
using Domain;

namespace IBusinessLogic.Strategy;

public interface IConcreteStrategy
{
    public string Name { get; }
    public int CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest);
}