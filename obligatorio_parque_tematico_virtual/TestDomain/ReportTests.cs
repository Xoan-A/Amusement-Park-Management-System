using Domain;

namespace TestDomain;

[TestClass]
public class ReportTests
{
    [TestMethod]
    public void Report_Constructor_ShouldInitializeWithEnterDateAndAttraction()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction = new Attraction
        {
            Name = "Roller Coaster",
            Type = AttractionType.RollerCoaster
        };

        Report report = new Report(enterDate, attraction);

        Assert.AreEqual(enterDate, report.EnterDate);
        Assert.IsNotNull(report.Attractions);
        Assert.AreEqual(1, report.Attractions.Count);
        Assert.AreEqual(attraction, report.Attractions[0]);
    }

    [TestMethod]
    public void SetExitTime_ShouldSetExitDate()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);
        Attraction attraction = new Attraction
        {
            Name = "Ferris Wheel",
            Type = AttractionType.Simulator
        };
        Report report = new Report(enterDate, attraction);

        report.SetExitTime(exitDate);

        Assert.AreEqual(exitDate, report.ExitDate);
    }
}