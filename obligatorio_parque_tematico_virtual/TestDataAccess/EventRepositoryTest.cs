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
    public void GetById_ShouldReturnEvent_WhenEventExists()
    {
        _context.Events.Add(eventEntity);
        _context.SaveChanges();

        Event result = _eventRepository.GetById(eventEntity.Id);

        Assert.AreEqual(eventEntity.Id, result.Id);
        Assert.AreEqual(eventEntity.Name, result.Name);
    }

    [TestMethod]
    public void GetById_ShouldReturnNull_WhenEventDoesNotExist()
    {
        Event result = _eventRepository.GetById(Guid.NewGuid());

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetById_ShouldIncludeAttractions_WhenEventHasAttractions()
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

        _context.Attractions.Add(attraction1);
        _context.Attractions.Add(attraction2);
        _context.SaveChanges();

        eventEntity.AddAttraction(attraction1);
        eventEntity.AddAttraction(attraction2);

        _context.Events.Add(eventEntity);
        _context.SaveChanges();

        Event result = _eventRepository.GetById(eventEntity.Id);

        Assert.AreEqual(2, result.Attractions.Count);
        Assert.IsTrue(result.Attractions.First().AttractionId == attraction1.Id);
        Assert.IsTrue(result.Attractions.Last().AttractionId == attraction2.Id);
    }

    [TestMethod]
    public void Create_ShouldAddEventToDatabase()
    {
        _eventRepository.Create(eventEntity);

        Event result = _context.Events.Find(eventEntity.Id);

        Assert.AreEqual(eventEntity.Name, result.Name);
    }

    [TestMethod]
    public void GetAll_ShouldReturnAllEvents_WhenEventsExist()
    {
        _context.Events.Add(eventEntity);
        _context.SaveChanges();

        List<Event> events = _eventRepository.GetAll();

        Assert.IsTrue(events.Any());
        Assert.AreEqual(1, events.Count());
        Assert.AreEqual(eventEntity.Name, events.First().Name);
    }

    [TestMethod]
    public void GetAll_ShouldIncludeAttractions_WhenEventsHaveAttractions()
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

        _context.Attractions.Add(attraction1);
        _context.Attractions.Add(attraction2);
        _context.SaveChanges();

        eventEntity.AddAttraction(attraction1);
        _context.Events.Add(eventEntity);

        Event newEvent = new Event
        {
            Name = "Water Park Day",
            Date = new DateTime(2024, 09, 20, 10, 0, 0),
            MaxCapacity = 2000
        };
        newEvent.AddAttraction(attraction2);
        _context.Events.Add(newEvent);

        _context.SaveChanges();

        List<Event> result = _eventRepository.GetAll();

        Assert.AreEqual(2, result.Count);

        Event firstEvent = result.First(e => e.Id == eventEntity.Id);
        Assert.AreEqual(1, firstEvent.Attractions.Count);
        Assert.IsTrue(firstEvent.Attractions.First().AttractionId == attraction1.Id);

        Event secondEvent = result.First(e => e.Id == newEvent.Id);
        Assert.AreEqual(1, secondEvent.Attractions.Count);
        Assert.IsTrue(secondEvent.Attractions.First().AttractionId == attraction2.Id);
    }

    [TestMethod]
    public void Delete_ShouldDeleteEventFromDatabase()
    {
        _eventRepository.Create(eventEntity);

        _eventRepository.Delete(eventEntity);

        Event result = _eventRepository.GetById(eventEntity.Id);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetEventByAttractionAndDate_ShouldReturnEvent_WhenEventExistsWithAttractionAndDate()
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

        _context.Events.Add(eventEntity);
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        Event? result = _eventRepository.GetEventByAttractionAndDate(attraction.Id, eventEntity.Date);

        Assert.AreEqual(eventEntity.Id, result.Id);
        Assert.IsTrue(result.Attractions.Any(ea => ea.AttractionId == attraction.Id));
    }

    [TestMethod]
    public void GetEventByAttractionAndDate_ShouldReturnNull_WhenNoEventExistsForDate()
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

        _context.Events.Add(eventEntity);
        _context.Attractions.Add(attraction);
        _context.SaveChanges();

        Event? result = _eventRepository.GetEventByAttractionAndDate(attraction.Id, new DateTime(2024, 9, 15));

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetEventByAttractionAndDate_ShouldReturnNull_WhenEventExistsButDoesNotHaveAttraction()
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

        _context.Events.Add(eventEntity);
        _context.Attractions.Add(attraction1);
        _context.Attractions.Add(attraction2);
        _context.SaveChanges();

        Event? result = _eventRepository.GetEventByAttractionAndDate(attraction2.Id, eventEntity.Date);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetEventByAttractionAndDate_ShouldReturnNull_WhenNoEventExists()
    {
        Event? result = _eventRepository.GetEventByAttractionAndDate(Guid.NewGuid(), new DateTime(2024, 8, 15));

        Assert.IsNull(result);
    }
}