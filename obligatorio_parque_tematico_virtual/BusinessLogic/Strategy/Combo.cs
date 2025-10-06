using IBusinessLogic.Strategy;
using Domain;

namespace BusinessLogic;

public class Combo : IContreteStrategy
{
    public string Name => "Combo";
    public int N { get; set; }

    public Combo(int n)
    {
        N = n;
    }

    public int CalculateScore(StrategyRequest strategyRequest)
    {
        User visitor = strategyRequest.User;
        Attraction attraction = strategyRequest.Attraction;
        DateTime? enterDate = strategyRequest.EnterDate;

        if (visitor == null || attraction == null || enterDate == null)
            throw new ArgumentException("Visitor, Attraction and EnterDate must be provided");

        int baseScore = ActiveStrategy.BasicCalculation(strategyRequest);

        List<Report> previousReports = visitor.VisitorReports
            .SelectMany(vr => vr.Reports)
            .Where(r => r.EnterDate < enterDate.Value)
            .OrderByDescending(r => r.EnterDate)
            .ToList();

        if (!previousReports.Any())
            return baseScore;

        Report previousReport = previousReports.First();

        TimeSpan timeDifference = enterDate.Value - previousReport.EnterDate;
        bool isDifferentAttraction = attraction.Id != previousReport.Attraction.Id;
        bool isWithinTimeWindow = timeDifference.TotalMinutes <= N && timeDifference.TotalMinutes >= 0;

        if (isDifferentAttraction && isWithinTimeWindow)
            return baseScore * 2;

        return baseScore;
    }
}