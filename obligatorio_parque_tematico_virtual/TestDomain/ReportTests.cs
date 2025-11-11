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
        Assert.AreEqual(attraction, report.Attraction);
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

    [TestMethod]
    public void Report_ExitDate_ShouldBeDefaultBeforeSet()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction = new Attraction { Name = "Test Attraction" };
        Report report = new Report(enterDate, attraction);

        Assert.AreEqual(null, report.ExitDate);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Report_ExitDate_CanNotBeSetBeforeStartDate()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        DateTime exitDate = new DateTime(2025, 10, 1, 9, 30, 0);
        Attraction attraction = new Attraction { Name = "Test Attraction" };
        Report report = new Report(enterDate, attraction);

        report.SetExitTime(exitDate);
    }

    [TestMethod]
    public void Report_ExitDate_WhenCorrectExitDateInsertedItAddsItToReport()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        DateTime exitDate = new DateTime(2025, 10, 1, 11, 30, 0);
        Attraction attraction = new Attraction { Name = "Test Attraction" };
        Report report = new Report(enterDate, attraction);

        report.SetExitTime(exitDate);

        Assert.AreEqual(exitDate, report.ExitDate);
    }

    [TestMethod]
    public void Report_ShouldStoreCorrectAttraction()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction = new Attraction
        {
            Name = "Bumper Cars",
            Type = AttractionType.InteractiveZone
        };
        Report report = new Report(enterDate, attraction);

        Assert.AreEqual(attraction, report.Attraction);
        Assert.AreEqual("Bumper Cars", report.Attraction.Name);
    }
}