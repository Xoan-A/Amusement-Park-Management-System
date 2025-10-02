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
    private Mock<IAttractionServiceEntity> _mockAttractionService;
    private Event baseEvent;
    private EventRequest baseEventRequest;

    [TestInitialize]
    public void Setup()
    {
        _mockEventRepository = new Mock<IEventRepository>();
        _mockAttractionService = new Mock<IAttractionServiceEntity>();
        _eventService = new EventService(_mockEventRepository.Object, _mockAttractionService.Object);
        baseEvent = new Event
        {
            Name = "Base Event",
            Date = new DateTime(2025, 10, 10),
            Hour = 10,
            MaxCapacity = 1000,
            CurrentCapacity = 100,
            Cost = 50,
            Attractions = new List<EventAttraction>()
        };
        baseEventRequest = new EventRequest
        {
            Name = "Base Event",
            Date = new DateTime(2025, 10, 10),
            Hour = 10,
            MaxCapacity = 1000,
            Cost = 50,
            AttractionIds = new List<Guid>()
        };
    }

    [TestMethod]
    public async Task GetEventById_ShouldReturnEvent_WhenIdIsValid()
    {
        _mockEventRepository.Setup(r => r.GetById(baseEvent.Id)).ReturnsAsync(baseEvent);
        EventResponse result = await _eventService.GetEventById(baseEvent.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(baseEvent.Name, result.Name);
        _mockEventRepository.Verify(r => r.GetById(baseEvent.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetEventById_ShouldReturnEventWithAttractions_WhenEventHasAttractions()
    {
        var attraction1 = new Attraction
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
        baseEvent.AddAttraction(attraction1);
        _mockEventRepository.Setup(r => r.GetById(baseEvent.Id)).ReturnsAsync(baseEvent);
        EventResponse result = await _eventService.GetEventById(baseEvent.Id);
        Assert.IsNotNull(result);
        Assert.AreEqual(baseEvent.Name, result.Name);
        Assert.AreEqual(1, result.Attractions.Count);
        Assert.IsTrue(result.Attractions.Any(a => a.Name == "Roller Coaster"));
        _mockEventRepository.Verify(r => r.GetById(baseEvent.Id), Times.Once);
    }

    [TestMethod]
    public async Task GetEventById_ShouldThrowException_WhenEventIsNull()
    {
        Guid eventId = Guid.NewGuid();
        _mockEventRepository.Setup(r => r.GetById(eventId)).ReturnsAsync((Event)null);

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () => await _eventService.GetEventById(eventId));
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldReturnListOfEvents()
    {
        var event1 = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Food Festival",
            Date = baseEvent.Date,
            Hour = baseEvent.Hour,
            MaxCapacity = baseEvent.MaxCapacity,
            CurrentCapacity = baseEvent.CurrentCapacity,
            Cost = baseEvent.Cost,
            Attractions = new List<EventAttraction>()
        };
        var event2 = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Art Expo",
            Date = baseEvent.Date,
            Hour = baseEvent.Hour,
            MaxCapacity = baseEvent.MaxCapacity,
            CurrentCapacity = baseEvent.CurrentCapacity,
            Cost = baseEvent.Cost,
            Attractions = new List<EventAttraction>()
        };
        List<Event> expectedEvents = new List<Event> { event1, event2 };
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(expectedEvents);
        List<EventResponse> result = await _eventService.GetAllEvents();
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Food Festival", result[0].Name);
        Assert.AreEqual("Art Expo", result[1].Name);
        _mockEventRepository.Verify(r => r.GetAll(), Times.Once);
    }

    [TestMethod]
    public async Task GetAllEvents_ShouldReturnEventsWithAttractions()
    {
        Attraction attraction = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Montaña Rusa",
            Description = "Atracción rápida",
            Type = AttractionType.RollerCoaster,
            MinAge = 10,
            MaxCapacity = 20,
            CurrentCapacity = 5,
            IsActive = true
        };
        EventAttraction eventAttraction = new EventAttraction { Attraction = attraction };
        Event eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Name = "Fiesta de Halloween",
            Date = new DateTime(2025, 10, 31),
            Hour = 20,
            MaxCapacity = 100,
            CurrentCapacity = 10,
            Cost = 50,
            Attractions = new List<EventAttraction> { eventAttraction }
        };
        List<Event> eventsList = new List<Event> { eventEntity };
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(eventsList);
        var eventService = new EventService(_mockEventRepository.Object, _mockAttractionService.Object);

        var result = await eventService.GetAllEvents();

        Assert.AreEqual(1, result.Count);
        EventResponse returnedEvent = result[0];
        Assert.AreEqual("Fiesta de Halloween", returnedEvent.Name);
        AttractionResponse returnedAttraction = returnedEvent.Attractions[0];
        Assert.AreEqual("Montaña Rusa", returnedAttraction.Name);
    }

    [TestMethod]
    public async Task CreateEvent_ShouldCreateEvent_WhenDataIsValid()
    {
        EventRequest newEvent = new EventRequest()
        {
            Name = "Tech Conference",
            Date = new DateTime(2025, 11, 15),
            Hour = 9,
            MaxCapacity = 1000,
            Cost = 150
        };

        Event createdEvent = new Event
        {
            Id = Guid.NewGuid(),
            Name = newEvent.Name,
            Date = newEvent.Date,
            Hour = newEvent.Hour,
            MaxCapacity = newEvent.MaxCapacity,
            CurrentCapacity = 0,
            Cost = newEvent.Cost,
            Attractions = new List<EventAttraction>()
        };

        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Event>());
        _mockEventRepository.Setup(r => r.Create(It.IsAny<Event>())).Callback<Event>(e => e.Id = createdEvent.Id)
            .Returns(Task.CompletedTask);

        Guid resultId = await _eventService.CreateEvent(newEvent);
        Assert.AreEqual(createdEvent.Id, resultId);
    }

    [TestMethod]
    public async Task CreateEvent_ShouldAddAttractions_WhenEventHasAttractions()
    {
        var attractionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        EventRequest newEvent = new EventRequest()
        {
            Name = "Carnival",
            Date = new DateTime(2025, 10, 3),
            Hour = 15,
            MaxCapacity = 4000,
            Cost = 80,
            AttractionIds = attractionIds
        };

        _mockAttractionService.Setup(s => s.GetAttractionEntityById(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new Attraction { Id = id, Name = $"Attraction-{id}" });


        _mockEventRepository.Setup(r => r.Create(It.IsAny<Event>()))
            .Callback<Event>(e =>
            {
                Assert.AreEqual(attractionIds.Count, e.Attractions.Count);
                Assert.AreEqual(attractionIds[0], e.Attractions[0].AttractionId);
                Assert.AreEqual(attractionIds[1], e.Attractions[1].AttractionId);
            })
            .Returns(Task.CompletedTask);

        await _eventService.CreateEvent(newEvent);
    }

    [TestMethod]
    public async Task CreateEvent_ShouldThrowException_WhenNameIsNotUnique()
    {
        var existingEvents = new List<Event>
        {
            new Event
            {
                Name = "Evento Unico", Date = DateTime.Now.AddDays(2), Hour = 10, MaxCapacity = 100,
                CurrentCapacity = 10, Cost = 10
            }
        };
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(existingEvents);
        EventRequest newEvent = new EventRequest
        {
            Name = "Evento Unico", Date = DateTime.Now.AddDays(3), Hour = 12, MaxCapacity = 100, Cost = 10
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
    }

    [TestMethod]
    public async Task CreateEvent_ShouldThrowException_WhenNameIsEmpty()
    {
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Event>());
        EventRequest newEvent = new EventRequest
        {
            Name = "", Date = DateTime.Now.AddDays(1), Hour = 10, MaxCapacity = 100, Cost = 10
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
    }

    [TestMethod]
    public async Task CreateEvent_ShouldThrowException_WhenDateIsNotFuture()
    {
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Event>());
        EventRequest newEvent = new EventRequest
        {
            Name = "Nuevo Evento", Date = DateTime.Now.AddMinutes(-1), Hour = 10, MaxCapacity = 100, Cost = 10
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
    }

    [TestMethod]
    public async Task CreateEvent_ShouldThrowException_WhenHourIsInvalid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Event>());
        EventRequest newEvent = new EventRequest
        {
            Name = "Nuevo Evento", Date = DateTime.Now.AddDays(1), Hour = 24, MaxCapacity = 100, Cost = 10
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
    }

    [TestMethod]
    public async Task CreateEvent_ShouldThrowException_WhenMaxCapacityIsInvalid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Event>());
        EventRequest newEvent = new EventRequest
        {
            Name = "Nuevo Evento", Date = DateTime.Now.AddDays(1), Hour = 10, MaxCapacity = 0, Cost = 10
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
        newEvent.MaxCapacity = 10001;
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
    }

    [TestMethod]
    public async Task CreateEvent_ShouldThrowException_WhenCostIsInvalid()
    {
        _mockEventRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<Event>());
        EventRequest newEvent = new EventRequest
        {
            Name = "Nuevo Evento", Date = DateTime.Now.AddDays(1), Hour = 10, MaxCapacity = 100, Cost = 0
        };
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _eventService.CreateEvent(newEvent));
    }

    [TestMethod]
    public async Task DeleteEvent_ShouldCallRepositoryDelete_WhenEventExists()
    {
        Guid eventId = Guid.NewGuid();
        Event existingEvent = new Event
        {
            Id = eventId,
            Name = "Sample Event",
            Date = DateTime.Now,
            Hour = 10,
            MaxCapacity = 100,
            CurrentCapacity = 0,
            Cost = 20,
            Attractions = []
        };

        _mockEventRepository.Setup(r => r.GetById(eventId)).ReturnsAsync(existingEvent);
        _mockEventRepository.Setup(r => r.Delete(existingEvent)).Returns(Task.CompletedTask);

        await _eventService.DeleteEvent(eventId);

        _mockEventRepository.Verify(r => r.GetById(eventId), Times.Once);
        _mockEventRepository.Verify(r => r.Delete(existingEvent), Times.Once);
    }

    [TestMethod]
    public async Task DeleteEvent_ShouldThrowException_WhenEventDoesNotExist()
    {
        Guid eventId = Guid.NewGuid();
        _mockEventRepository.Setup(r => r.GetById(eventId)).ReturnsAsync((Event)null);
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () => await _eventService.DeleteEvent(eventId));
    }
}