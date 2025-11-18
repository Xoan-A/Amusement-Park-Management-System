using IBusinessLogic.Strategy;
using Domain;
using Models.In;

namespace BusinessLogic;

public class Combo : IConcreteStrategy
{
    public string Name => "Combo";
    public int N { get; set; }

    public Combo(int n)
    {
        N = n;
    }

    public int CalculateScore(User visitor, Attraction attraction, StrategyRequest strategyRequest)
    {

        if (visitor == null || attraction == null || strategyRequest.EnterDate == null)
            throw new ArgumentException("Visitor, Attraction and EnterDate must be provided");

        int baseScore = ActiveStrategy.BasicCalculation(visitor, attraction);

        List<Report> previousReports = visitor.VisitorReports
            .SelectMany(vr => vr.Reports)
            .Where(r => r.EnterDate < strategyRequest.EnterDate.Value)
            .OrderByDescending(r => r.EnterDate)
            .ToList();

        if (!previousReports.Any())
            return baseScore;

        Report previousReport = previousReports.First();

        TimeSpan timeDifference = strategyRequest.EnterDate.Value - previousReport.EnterDate;
        bool isDifferentAttraction = attraction.Id != previousReport.Attraction.Id;
        bool isWithinTimeWindow = timeDifference.TotalMinutes <= N && timeDifference.TotalMinutes >= 0;

        if (isDifferentAttraction && isWithinTimeWindow)
            return baseScore * 2;

        return baseScore;
    }
}