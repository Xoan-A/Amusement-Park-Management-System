using IBusinessLogic.Strategy;
using Domain;
using BusinessLogic;
using BusinessLogic.Plugins;

namespace ExamplePlugin;

[PluginDescription("Multiplies scoring points based on hour of the day. 2x during peak hours (10-14, 18-22), 1.5x during normal hours, 1x during off-peak hours.")]
[PluginAuthor("Theme Park Team")]
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
            // Double points during peak hours
            return baseScore * 2;
        }
        else if (_normalHours.Contains(currentHour))
        {
            // 1.5x points during normal hours
            return (int)(baseScore * 1.5);
        }
        else
        {
            // Regular points during off-peak hours (0-8, 23)
            return baseScore;
        }
    }
}
