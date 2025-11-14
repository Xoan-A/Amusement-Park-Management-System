using Microsoft.EntityFrameworkCore;
using Domain;
using DataAccess.Context;
using DataAccess.Repositories;
using IDataAccess;

namespace TestDataAccess;

[TestClass]
public class AttractionRepositoryTest
{
    private AppDbContext _context;
    private IAttractionRepository _attractionRepository;
    private Attraction attraction;

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
        _attractionRepository = new AttractionRepository(_context);

        attraction = new Attraction();

        attraction.Name = "Race simulator";
        attraction.Description = "average race simulator";
        attraction.Type = AttractionType.Simulator;
        attraction.MinAge = 18;
        attraction.MaxCapacity = 10;
        attraction.CurrentCapacity = 0;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [TestMethod]
    public void GetById_ShouldReturnAttraction_WhenAttractionExists()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        Attraction result = _attractionRepository.GetById(attraction.Id);

        Assert.AreEqual(attraction.Id, result.Id);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenAttractionDoesNotExist()
    {
        Guid nonExistentId = Guid.NewGuid();

        Attraction result = _attractionRepository.GetById(nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Create_ShouldAddAttractionToDatabase()
    {
        _attractionRepository.Create(attraction);
        Attraction result = _attractionRepository.GetById(attraction.Id);

        Assert.AreEqual("Race simulator", result.Name);
        Assert.AreEqual(1, _context.Attractions.Count());
    }

    [TestMethod]
    public void GetByName_ShouldReturnAttraction_WhenAttractionExists()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        Attraction result = _attractionRepository.GetByName("Race simulator");

        Assert.AreEqual(attraction.Name, result.Name);
    }

    [TestMethod]
    public void GetByName_ShouldReturnNull_WhenAttractionDoesNotExist()
    {
        Attraction result = _attractionRepository.GetByName("nonexistent");

        Assert.IsNull(result);
    }

    [TestMethod]
    public void IsNameUnique_ShouldReturnFalse_WhenNameExists()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        bool result = _attractionRepository.IsNameUnique("Race simulator");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsNameUnique_ShouldReturnTrue_WhenNameDoesNotExist()
    {
        bool result = _attractionRepository.IsNameUnique("new name");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void GetAll_ShouldReturnAllAttractions()
    {
        Attraction attraction2 = new Attraction
        {
            Name = "Haunted House",
            Description = "A spooky experience",
            Type = AttractionType.Simulator,
            MinAge = 8,
            MaxCapacity = 15,
            CurrentCapacity = 3,
        };

        _context.Attractions.Add(attraction);
        _context.Attractions.Add(attraction2);
        _context.SaveChanges();
        List<Attraction> result = _attractionRepository.GetAll();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void Update_ShouldUpdateAttractionInDatabase()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        attraction.Name = "Updated Name";
        _attractionRepository.Update(attraction);

        Attraction result = _attractionRepository.GetById(attraction.Id);

        Assert.AreEqual("Updated Name", result.Name);
    }

    [TestMethod]
    public void Delete_ShouldDeleteAttractionFromDatabase()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        _attractionRepository.Delete(attraction);

        Assert.AreEqual(0, _context.Attractions.Count());
    }

    [TestMethod]
    public void Delete_ShouldDeleteMaintenanceSchedules_BeforeDeletingAttraction()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        MaintenanceSchedule maintenance1 = new MaintenanceSchedule
        {
            Id = Guid.NewGuid(),
            AttractionId = attraction.Id,
            ScheduledDate = DateTime.Now.AddDays(1),
            Status = MaintenanceStatus.Pending
        };

        MaintenanceSchedule maintenance2 = new MaintenanceSchedule
        {
            Id = Guid.NewGuid(),
            AttractionId = attraction.Id,
            ScheduledDate = DateTime.Now.AddDays(2),
            Status = MaintenanceStatus.Pending
        };

        _context.MaintenanceSchedules.Add(maintenance1);
        _context.MaintenanceSchedules.Add(maintenance2);
        _context.SaveChanges();

        Assert.AreEqual(2, _context.MaintenanceSchedules.Count());

        _attractionRepository.Delete(attraction);

        Assert.AreEqual(0, _context.Attractions.Count());
        Assert.AreEqual(0, _context.MaintenanceSchedules.Count());
    }

    [TestMethod]
    public void Delete_ShouldDeleteEventAttractions_BeforeDeletingAttraction()
    {
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        Event event1 = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Special Event 1",
            Date = DateTime.Now.AddDays(5)
        };

        Event event2 = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Special Event 2",
            Date = DateTime.Now.AddDays(10)
        };

        _context.Events.Add(event1);
        _context.Events.Add(event2);
        _context.SaveChanges();

        EventAttraction eventAttraction1 = new EventAttraction
        {
            EventId = event1.Id,
            AttractionId = attraction.Id
        };

        EventAttraction eventAttraction2 = new EventAttraction
        {
            EventId = event2.Id,
            AttractionId = attraction.Id
        };

        _context.Set<EventAttraction>().Add(eventAttraction1);
        _context.Set<EventAttraction>().Add(eventAttraction2);
        _context.SaveChanges();

        Assert.AreEqual(2, _context.Set<EventAttraction>().Count());

        _attractionRepository.Delete(attraction);

        Assert.AreEqual(0, _context.Attractions.Count());
        Assert.AreEqual(0, _context.Set<EventAttraction>().Count());
        Assert.AreEqual(2, _context.Events.Count());
    }
}