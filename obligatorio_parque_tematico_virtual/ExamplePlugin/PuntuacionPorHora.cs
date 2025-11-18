using IBusinessLogic.Strategy;
using Domain;
using BusinessLogic;
using Models.In;

namespace ExamplePlugin;
public class PuntuacionPorHora : IConcreteStrategy
{
    public string Name => "PuntuacionPorHora";

    private readonly int[] _peakHours = { 10, 11, 12, 13, 18, 19, 20, 21 };
    private readonly int[] _normalHours = { 9, 14, 15, 16, 17, 22 };

    public int CalculateScore(User user, Attraction attraction, StrategyRequest strategyRequest)
    {
        int baseScore = ActiveStrategy.BasicCalculation(user, attraction);

        DateTime currentTime = strategyRequest.EnterDate ?? DateTime.Now;
        int currentHour = currentTime.Hour;

        if (_peakHours.Contains(currentHour))
        {
            return baseScore * 2;
        }
        else if (_normalHours.Contains(currentHour))
        {
            return (int)(baseScore * 1.5);
        }
        else
        {
            return baseScore;
        }
    }
}