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

    [TestMethod]
    public void Report_ExitDate_ShouldBeDefaultBeforeSet()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction = new Attraction { Name = "Test Attraction" };
        Report report = new Report(enterDate, attraction);

        Assert.AreEqual(default(DateTime), report.ExitDate);
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
    public void AddAttraction_ShouldAddAttractionToList()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction firstAttraction = new Attraction
        {
            Name = "Bumper Cars",
            Type = AttractionType.InteractiveZone
        };
        Attraction secondAttraction = new Attraction
        {
            Name = "Haunted House",
            Type = AttractionType.Performance
        };
        Report report = new Report(enterDate, firstAttraction);

        report.AddAttraction(secondAttraction);

        Assert.AreEqual(2, report.Attractions.Count);
        Assert.AreEqual(firstAttraction, report.Attractions[0]);
        Assert.AreEqual(secondAttraction, report.Attractions[1]);
    }

    [TestMethod]
    public void AddAttraction_ShouldAddMultipleAttractions()
    {
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction1 = new Attraction { Name = "Attraction 1" };
        Attraction attraction2 = new Attraction { Name = "Attraction 2" };
        Attraction attraction3 = new Attraction { Name = "Attraction 3" };
        Report report = new Report(enterDate, attraction1);

        report.AddAttraction(attraction2);
        report.AddAttraction(attraction3);

        Assert.AreEqual(3, report.Attractions.Count);
        Assert.AreEqual(attraction1, report.Attractions[0]);
        Assert.AreEqual(attraction2, report.Attractions[1]);
        Assert.AreEqual(attraction3, report.Attractions[2]);
    }
}