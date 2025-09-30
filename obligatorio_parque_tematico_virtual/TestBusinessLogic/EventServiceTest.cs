using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

namespace TestBusinessLogic;

[TestClass]
public class EventServiceTest
{
    private Mock<IEventRepository> _mockEventRepository;
    private IEventService _eventService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _eventService = new EventService(_mockEventRepository.Object);
    }

    [TestMethod]
    public async Task GetEventById_ShouldReturnEvent_WhenIdIsValid()
    {
        Event expectedEvent = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Music Festival",
            Date = new DateTime(2024, 8, 15),
            Hour = 10,
            MaxCapacity = 5000,
            CurrentCapacity = 0,
            Cost = 100,
            Attractions = []
        };
        _mockEventRepository.Setup(r => r.GetById(expectedEvent.Id)).ReturnsAsync(expectedEvent);
        EventResponse result = await _eventService.GetEventById(expectedEvent.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedEvent.Name, result.Name);
        _mockEventRepository.Verify(r => r.GetById(expectedEvent.Id), Times.Once);
    }
    
    [TestMethod]
    public async Task GetEventById_ShouldReturnEventWithAttractions_WhenEventHasAttractions()
    {
        Attraction attraction1 = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Roller Coaster",
            Description = "A thrilling ride",
            Type = AttractionType.RollerCoaster,
            MinAge = 12,
            MaxCapacity = 20,
            CurrentCapacity = 5,
            IsActive = true
        };
        
        Event expectedEvent = new Event
        {
            Name = "Fun Fair",
            Date = new DateTime(2024, 9, 10),
            Hour = 14,
            MaxCapacity = 3000,
            CurrentCapacity = 0,
            Cost = 50,
        };
        
        expectedEvent.AddAttraction(attraction1);
        
        _mockEventRepository.Setup(r => r.GetById(expectedEvent.Id)).ReturnsAsync(expectedEvent);
        EventResponse result = await _eventService.GetEventById(expectedEvent.Id);
        
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedEvent.Name, result.Name);
        Assert.AreEqual(1, result.Attractions.Count);
        Assert.IsTrue(result.Attractions.Any(a => a.Name == "Roller Coaster"));
        
        _mockEventRepository.Verify(r => r.GetById(expectedEvent.Id), Times.Once);
    }
    
    [TestMethod]
    public async Task GetAllEvents_ShouldReturnListOfEvents()
    {
        List<Event> expectedEvents = new List<Event>
        {
            new Event
            {
                Name = "Food Festival",
                Date = new DateTime(2024, 7, 20),
                Hour = 12,
                MaxCapacity = 2000,
                CurrentCapacity = 0,
                Cost = 30,
                Attractions = []
            },
            new Event
            {
                Name = "Art Expo",
                Date = new DateTime(2024, 10, 5),
                Hour = 9,
                MaxCapacity = 1500,
                CurrentCapacity = 0,
                Cost = 20,
                Attractions = []
            }
        };
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(expectedEvents);
        List<EventResponse> result = await _eventService.GetAllEvents();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Food Festival", result[0].Name);
        Assert.AreEqual("Art Expo", result[1].Name);
        _mockEventRepository.Verify(r => r.GetAll(), Times.Once);
    }