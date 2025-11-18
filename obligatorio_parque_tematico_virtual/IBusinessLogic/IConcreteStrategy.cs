using Models.In;
using Domain;

namespace IBusinessLogic;

public interface IConcreteStrategy
{
    public string Name { get; }
    public int CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest);
}