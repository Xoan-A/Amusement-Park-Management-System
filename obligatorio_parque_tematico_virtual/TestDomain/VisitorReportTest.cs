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
        Assert.IsNotNull(visitorReport.Reports);
        Assert.AreEqual(1, visitorReport.Reports.Count);
        Assert.AreEqual(report, visitorReport.Reports[0]);
    }
}