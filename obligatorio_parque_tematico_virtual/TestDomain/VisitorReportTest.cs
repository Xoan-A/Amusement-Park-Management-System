using Domain;

namespace TestDomain;

[TestClass]
public class VisitorReportTests
{
    [TestMethod]
    public void VisitorReport_Constructor_ShouldInitializeWithDateAndReport()
    {
        DateTime date = new DateTime(2025, 10, 1);
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction = new Attraction
        {
            Name = "Roller Coaster",
            Type = AttractionType.RollerCoaster
        };
        Report report = new Report(enterDate, attraction);

        VisitorReport visitorReport = new VisitorReport(date, report);

        Assert.AreEqual(date, visitorReport.Date);
        Assert.AreEqual(1, visitorReport.Reports.Count);
        Assert.AreEqual(report, visitorReport.Reports[0]);
    }

    [TestMethod]
    public void AddReport_ShouldAddReportToList()
    {
        DateTime date = new DateTime(2025, 10, 1);
        DateTime enterDate1 = new DateTime(2025, 10, 1, 10, 0, 0);
        DateTime enterDate2 = new DateTime(2025, 10, 1, 14, 0, 0);
        Attraction attraction1 = new Attraction
        {
            Name = "Bumper Cars",
            Type = AttractionType.InteractiveZone
        };
        Attraction attraction2 = new Attraction
        {
            Name = "Haunted House",
            Type = AttractionType.Performance
        };
        Report firstReport = new Report(enterDate1, attraction1);
        Report secondReport = new Report(enterDate2, attraction2);
        VisitorReport visitorReport = new VisitorReport(date, firstReport);

        visitorReport.AddReport(secondReport);

        Assert.AreEqual(2, visitorReport.Reports.Count);
        Assert.AreEqual(firstReport, visitorReport.Reports[0]);
        Assert.AreEqual(secondReport, visitorReport.Reports[1]);
    }

    [TestMethod]
    public void AddReport_ShouldAddMultipleReports()
    {
        DateTime date = new DateTime(2025, 10, 1);
        DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
        Attraction attraction = new Attraction { Name = "Test Attraction" };
        Report report1 = new Report(enterDate, attraction);
        Report report2 = new Report(enterDate.AddHours(2), attraction);
        Report report3 = new Report(enterDate.AddHours(4), attraction);
        VisitorReport visitorReport = new VisitorReport(date, report1);

        visitorReport.AddReport(report2);
        visitorReport.AddReport(report3);

        Assert.AreEqual(3, visitorReport.Reports.Count);
        Assert.AreEqual(report1, visitorReport.Reports[0]);
        Assert.AreEqual(report2, visitorReport.Reports[1]);
        Assert.AreEqual(report3, visitorReport.Reports[2]);
    }
}