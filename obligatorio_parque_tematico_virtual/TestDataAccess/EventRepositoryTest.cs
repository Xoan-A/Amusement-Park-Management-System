using Microsoft.EntityFrameworkCore;
using Domain;
using DataAccess.Context;
using DataAccess.Repositories;
using IDataAccess;

namespace TestDataAccess;

[TestClass]
public class EventRepositoryTest
{
    private AppDbContext _context;
    private IEventRepository _eventRepository;
    private Event eventEntity;

    [TestInitialize]
    public void Setup()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        
        _eventRepository = new EventRepository(_context);
        
        eventEntity = new Event
        {
            Name = "Music Festival",
            Date = new DateTime(2024, 8, 15),
            Hour = 10,
            MaxCapacity = 5000,
            CurrentCapacity = 0,
            Cost = 100
        };
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetById_ShouldReturnEvent_WhenEventExists()
    {
        await _context.Events.AddAsync(eventEntity);
        await _context.SaveChangesAsync();
        
        Event result = await _eventRepository.GetById(eventEntity.Id);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(eventEntity.Id, result.Id);
        Assert.AreEqual(eventEntity.Name, result.Name);
    }
}