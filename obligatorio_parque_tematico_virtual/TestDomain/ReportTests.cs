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
}