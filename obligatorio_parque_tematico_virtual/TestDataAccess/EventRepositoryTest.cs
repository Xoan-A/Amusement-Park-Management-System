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
            .UseSqlite("DataSource=:memory:")
            .Options;
        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

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
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetById_ShouldReturnEvent_WhenEventExists()
    {
        await _context.Events.AddAsync(eventEntity);
        await _context.SaveChangesAsync();

        Event result = await _eventRepository.GetById(eventEntity.Id);

        Assert.AreEqual(eventEntity.Id, result.Id);
        Assert.AreEqual(eventEntity.Name, result.Name);
    }

    [TestMethod]
    public async Task GetById_ShouldReturnNull_WhenEventDoesNotExist()
    {
        Event result = await _eventRepository.GetById(Guid.NewGuid());

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetById_ShouldIncludeAttractions_WhenEventHasAttractions()
    {
        Attraction attraction1 = new Attraction
        {
            Name = "Roller Coaster",
            Description = "Fast ride",
            MinAge = 12,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };

        Attraction attraction2 = new Attraction
        {
            Name = "Ferris Wheel",
            Description = "Slow ride",
            MinAge = 5,
            MaxCapacity = 150,
            CurrentCapacity = 0
        };

        await _context.Attractions.AddAsync(attraction1);
        await _context.Attractions.AddAsync(attraction2);
        await _context.SaveChangesAsync();

        eventEntity.AddAttraction(attraction1);
        eventEntity.AddAttraction(attraction2);

        await _context.Events.AddAsync(eventEntity);
        await _context.SaveChangesAsync();

        Event result = await _eventRepository.GetById(eventEntity.Id);

        Assert.AreEqual(2, result.Attractions.Count);
        Assert.IsTrue(result.Attractions.First().AttractionId == attraction1.Id);
        Assert.IsTrue(result.Attractions.Last().AttractionId == attraction2.Id);
    }

    [TestMethod]
    public async Task Create_ShouldAddEventToDatabase()
    {
        await _eventRepository.Create(eventEntity);

        Event result = await _context.Events.FindAsync(eventEntity.Id);

        Assert.AreEqual(eventEntity.Name, result.Name);
    }

    [TestMethod]
    public async Task GetAll_ShouldReturnAllEvents_WhenEventsExist()
    {
        await _context.Events.AddAsync(eventEntity);
        await _context.SaveChangesAsync();

        List<Event> events = await _eventRepository.GetAll();

        Assert.IsTrue(events.Any());
        Assert.AreEqual(1, events.Count());
        Assert.AreEqual(eventEntity.Name, events.First().Name);
    }

    [TestMethod]
    public async Task GetAll_ShouldIncludeAttractions_WhenEventsHaveAttractions()
    {
        Attraction attraction1 = new Attraction
        {
            Name = "Haunted House",
            Description = "Scary ride",
            MinAge = 10,
            MaxCapacity = 50,
            CurrentCapacity = 0
        };

        Attraction attraction2 = new Attraction
        {
            Name = "Water Slide",
            Description = "Wet ride",
            MinAge = 8,
            MaxCapacity = 200,
            CurrentCapacity = 0
        };

        await _context.Attractions.AddAsync(attraction1);
        await _context.Attractions.AddAsync(attraction2);
        await _context.SaveChangesAsync();

        eventEntity.AddAttraction(attraction1);
        await _context.Events.AddAsync(eventEntity);

        Event newEvent = new Event
        {
            Name = "Water Park Day",
            Date = new DateTime(2024, 09, 20, 10, 0, 0),
            MaxCapacity = 2000
        };
        newEvent.AddAttraction(attraction2);
        await _context.Events.AddAsync(newEvent);

        await _context.SaveChangesAsync();

        List<Event> result = await _eventRepository.GetAll();

        Assert.AreEqual(2, result.Count);
        
        Event firstEvent = result.First(e => e.Id == eventEntity.Id);
        Assert.AreEqual(1, firstEvent.Attractions.Count);
        Assert.IsTrue(firstEvent.Attractions.First().AttractionId == attraction1.Id);

        Event secondEvent = result.First(e => e.Id == newEvent.Id);
        Assert.AreEqual(1, secondEvent.Attractions.Count);
        Assert.IsTrue(secondEvent.Attractions.First().AttractionId == attraction2.Id);
    }

    [TestMethod]
    public async Task Delete_ShouldDeleteEventFromDatabase()
    {
        await _eventRepository.Create(eventEntity);

        await _eventRepository.Delete(eventEntity);

        Event result = await _eventRepository.GetById(eventEntity.Id);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetEventByAttractionAndDate_ShouldReturnEvent_WhenEventExistsWithAttractionAndDate()
    {
        Attraction attraction = new Attraction
        {
            Name = "Roller Coaster",
            Description = "Fast ride",
            MinAge = 12,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };

        EventAttraction eventAttraction = new EventAttraction
        {
            Event = eventEntity,
            Attraction = attraction
        };

        eventEntity.Attractions = new List<EventAttraction> { eventAttraction };

        await _context.Events.AddAsync(eventEntity);
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        Event? result = await _eventRepository.GetEventByAttractionAndDate(attraction.Id, eventEntity.Date);

        Assert.AreEqual(eventEntity.Id, result.Id);
        Assert.IsTrue(result.Attractions.Any(ea => ea.AttractionId == attraction.Id));
    }

    [TestMethod]
    public async Task GetEventByAttractionAndDate_ShouldReturnNull_WhenNoEventExistsForDate()
    {
        Attraction attraction = new Attraction
        {
            Name = "Roller Coaster",
            Description = "Fast ride",
            MinAge = 12,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };

        EventAttraction eventAttraction = new EventAttraction
        {
            Event = eventEntity,
            Attraction = attraction
        };

        eventEntity.Attractions = new List<EventAttraction> { eventAttraction };

        await _context.Events.AddAsync(eventEntity);
        await _context.Attractions.AddAsync(attraction);
        await _context.SaveChangesAsync();

        Event? result = await _eventRepository.GetEventByAttractionAndDate(attraction.Id, new DateTime(2024, 9, 15));

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetEventByAttractionAndDate_ShouldReturnNull_WhenEventExistsButDoesNotHaveAttraction()
    {
        Attraction attraction1 = new Attraction
        {
            Name = "Roller Coaster",
            Description = "Fast ride",
            MinAge = 12,
            MaxCapacity = 100,
            CurrentCapacity = 0
        };

        Attraction attraction2 = new Attraction
        {
            Name = "Ferris Wheel",
            Description = "Slow ride",
            MinAge = 5,
            MaxCapacity = 150,
            CurrentCapacity = 0
        };

        EventAttraction eventAttraction = new EventAttraction
        {
            Event = eventEntity,
            Attraction = attraction1
        };

        eventEntity.Attractions = new List<EventAttraction> { eventAttraction };

        await _context.Events.AddAsync(eventEntity);
        await _context.Attractions.AddAsync(attraction1);
        await _context.Attractions.AddAsync(attraction2);
        await _context.SaveChangesAsync();

        Event? result = await _eventRepository.GetEventByAttractionAndDate(attraction2.Id, eventEntity.Date);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetEventByAttractionAndDate_ShouldReturnNull_WhenNoEventExists()
    {
        Event? result = await _eventRepository.GetEventByAttractionAndDate(Guid.NewGuid(), new DateTime(2024, 8, 15));

        Assert.IsNull(result);
    }
}