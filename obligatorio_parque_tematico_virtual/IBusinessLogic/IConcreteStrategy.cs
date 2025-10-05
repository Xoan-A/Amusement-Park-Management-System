using BusinessLogic;
using Domain;

namespace IBusinessLogic.Strategy;

public interface IContreteStrategy
{
    public string Name { get; }
    public int CalculateScore(StrategyRequest strategyRequest);
}