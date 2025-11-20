using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess;

[TestClass]
public class ReportRepositoryTest
{
    private AppDbContext _context;
    private IReportRepository _reportRepository;

    [TestInitialize]
    public void Setup()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite("DataSource=:memory:")
        .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        _reportRepository = new ReportRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [TestMethod]
    public void GetAllReports_ShouldReturnAllReportsWithAttractions()
    {
        Guid attraction1Id = Guid.NewGuid();
        Guid attraction2Id = Guid.NewGuid();

        Attraction attraction1 = new Attraction
        {
            Id = attraction1Id,
            Name = "A",
            Description = "Atracción A",
            Type = AttractionType.RollerCoaster,
            MinAge = 10,
            MaxCapacity = 50,
            CurrentCapacity = 0
        };

        Attraction attraction2 = new Attraction
        {
            Id = attraction2Id,
            Name = "B",
            Description = "Atracción B",
            Type = AttractionType.Simulator,
            MinAge = 5,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };

        _context.Attractions.Add(attraction1);
        _context.Attractions.Add(attraction2);
        _context.SaveChanges();

        User user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            LastName = "Pérez",
            Email = "juan.perez@example.com",
            Password = "password123",
            BirthDate = new DateTime(1990, 5, 15),
            MembershipLevel = MembershipLevel.Premium,
            Score = 100
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        Guid visitorReport1Id = Guid.NewGuid();
        Guid visitorReport2Id = Guid.NewGuid();

        VisitorReport visitorReport1 = new VisitorReport
        {
            Id = visitorReport1Id,
            Date = DateTime.Now.AddHours(-2),
            VisitorId = user.Id
        };

        VisitorReport visitorReport2 = new VisitorReport
        {
            Id = visitorReport2Id,
            Date = DateTime.Now.AddHours(-3),
            VisitorId = user.Id
        };

        _context.VisitorReports.Add(visitorReport1);
        _context.VisitorReports.Add(visitorReport2);
        _context.SaveChanges();

        Report report1 = new Report
        {
            Id = Guid.NewGuid(),
            AttractionId = attraction1Id,
            EnterDate = DateTime.Now.AddHours(-2),
            ExitDate = DateTime.Now.AddHours(-1),
            VisitorReportId = visitorReport1Id
        };

        Report report2 = new Report
        {
            Id = Guid.NewGuid(),
            AttractionId = attraction2Id,
            EnterDate = DateTime.Now.AddHours(-3),
            ExitDate = DateTime.Now.AddHours(-2),
            VisitorReportId = visitorReport2Id
        };

        _context.Reports.Add(report1);
        _context.Reports.Add(report2);
        _context.SaveChanges();

        List<Report> reports = _reportRepository.GetAllReports();

        Assert.AreEqual(2, reports.Count);
        Assert.IsTrue(reports.Any(r => r.Id == report1.Id && r.Attraction.Name == "A"));
        Assert.IsTrue(reports.Any(r => r.Id == report2.Id && r.Attraction.Name == "B"));
    }

    [TestMethod]
    public void GetAllReports_ShouldReturnEmptyList_WhenNoReportsExist()
    {
        List<Report> reports = _reportRepository.GetAllReports();

        Assert.AreEqual(0, reports.Count);
    }
}