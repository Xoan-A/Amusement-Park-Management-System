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
    public async Task GetById_ShouldReturnAttraction_WhenAttractionExists()
    {
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        Attraction result = await _attractionRepository.GetById(attraction.Id);

        Assert.AreEqual(attraction.Id, result.Id);
    }

    [TestMethod]
    public async Task GetById_ShouldReturnNull_WhenAttractionDoesNotExist()
    {
        Guid nonExistentId = Guid.NewGuid();

        Attraction result = await _attractionRepository.GetById(nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task Create_ShouldAddAttractionToDatabase()
    {
        await _attractionRepository.Create(attraction);
        Attraction result = await _attractionRepository.GetById(attraction.Id);

        Assert.AreEqual("Race simulator", result.Name);
        Assert.AreEqual(1, await _context.Attractions.CountAsync());
    }

    [TestMethod]
    public async Task GetByName_ShouldReturnAttraction_WhenAttractionExists()
    {
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        Attraction result = await _attractionRepository.GetByName("Race simulator");

        Assert.AreEqual(attraction.Name, result.Name);
    }

    [TestMethod]
    public async Task GetByName_ShouldReturnNull_WhenAttractionDoesNotExist()
    {
        Attraction result = await _attractionRepository.GetByName("nonexistent");

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task IsNameUnique_ShouldReturnFalse_WhenNameExists()
    {
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        bool result = await _attractionRepository.IsNameUnique("Race simulator");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsNameUnique_ShouldReturnTrue_WhenNameDoesNotExist()
    {
        bool result = await _attractionRepository.IsNameUnique("new name");

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetAll_ShouldReturnAllAttractions()
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

        await _context.Attractions.AddAsync(attraction);
        await _context.Attractions.AddAsync(attraction2);
        await _context.SaveChangesAsync();
        List<Attraction> result = await _attractionRepository.GetAll();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task Update_ShouldUpdateAttractionInDatabase()
    {
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        attraction.Name = "Updated Name";
        await _attractionRepository.Update(attraction);

        Attraction result = await _attractionRepository.GetById(attraction.Id);

        Assert.AreEqual("Updated Name", result.Name);
    }

    [TestMethod]
    public async Task Remove_ShouldRemoveAttractionFromDatabase()
    {
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        await _attractionRepository.Delete(attraction);

        Assert.AreEqual(0, await _context.Attractions.CountAsync());
    }
}