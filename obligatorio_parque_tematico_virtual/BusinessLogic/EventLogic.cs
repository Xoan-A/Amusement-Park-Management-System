using IBusinessLogic;
using IDataAccess;
using Models.Out;
using Domain;
using Models.In;

namespace BusinessLogic;

public class EventLogic : IEventLogic
{
    private IEventRepository _eventRepository;
    private IAttractionLogicEntity _attractionLogic;
    private readonly IDateTimeLogic _dateTimeLogic;
    private const int MinCapacityLimit = 1;
    private const int MaxCapacityLimit = 10000;
    private const int MinHour = 0;
    private const int MaxHour = 23;
    private const int MinCost = 1;

    public EventLogic(IEventRepository eventRepository, IAttractionLogicEntity attractionLogic,
        IDateTimeLogic dateTimeLogic)
    {
        _eventRepository = eventRepository;
        _attractionLogic = attractionLogic;
        _dateTimeLogic = dateTimeLogic;
    }

    public async Task<EventResponse> GetEventById(Guid expectedEventId)
    {
        Event eventEntity = await _eventRepository.GetById(expectedEventId);
        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con id {expectedEventId}");
        }

        EventResponse eventResponse = new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Date = eventEntity.Date,
            Hour = eventEntity.Hour,
            MaxCapacity = eventEntity.MaxCapacity,
            CurrentCapacity = eventEntity.CurrentCapacity,
            Cost = eventEntity.Cost,
            Attractions = eventEntity.Attractions
            .Select(ea => new AttractionResponse
            {
                Id = ea.Attraction.Id,
                Name = ea.Attraction.Name,
                Description = ea.Attraction.Description,
                Type = ea.Attraction.Type.ToString(),
                MinAge = ea.Attraction.MinAge,
                MaxCapacity = ea.Attraction.MaxCapacity,
                CurrentCapacity = ea.Attraction.CurrentCapacity,
                IsActive = ea.Attraction.IsActive
            })
            .ToList()
        };
        return eventResponse;
    }

    public async Task<List<EventResponse>> GetAllEvents()
    {
        List<Event> events = await _eventRepository.GetAll();
        List<EventResponse> eventResponses = events.Select(eventEntity => new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Date = eventEntity.Date,
            Hour = eventEntity.Hour,
            MaxCapacity = eventEntity.MaxCapacity,
            CurrentCapacity = eventEntity.CurrentCapacity,
            Cost = eventEntity.Cost,
            Attractions = eventEntity.Attractions
            .Select(ea => new AttractionResponse
            {
                Id = ea.Attraction.Id,
                Name = ea.Attraction.Name,
                Description = ea.Attraction.Description,
                Type = ea.Attraction.Type.ToString(),
                MinAge = ea.Attraction.MinAge,
                MaxCapacity = ea.Attraction.MaxCapacity,
                CurrentCapacity = ea.Attraction.CurrentCapacity,
                IsActive = ea.Attraction.IsActive
            })
            .ToList()
        }).ToList();
        return eventResponses;
    }

    public async Task<Guid> CreateEvent(EventRequest newEvent)
    {
        await ValidateEventRequest(newEvent);
        Event eventEntity = new Event()
        {
            Name = newEvent.Name,
            Date = newEvent.Date,
            Hour = newEvent.Hour,
            MaxCapacity = newEvent.MaxCapacity,
            CurrentCapacity = 0,
            Cost = newEvent.Cost,
            Attractions = new List<EventAttraction>()
        };
        if (newEvent.AttractionIds != null)
        {
            foreach (Guid attractionId in newEvent.AttractionIds)
            {
                Attraction attraction = await _attractionLogic.GetAttractionEntityById(attractionId);
                eventEntity.AddAttraction(attraction);
            }
        }

        await _eventRepository.Create(eventEntity);
        return eventEntity.Id;
    }

    public async Task DeleteEvent(Guid eventId)
    {
        Event eventEntity = await _eventRepository.GetById(eventId);
        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con id {eventId}");
        }

        await _eventRepository.Delete(eventEntity);
    }

    private async Task ValidateEventRequest(EventRequest newEvent)
    {
        if (string.IsNullOrWhiteSpace(newEvent.Name))
            throw new ArgumentException("El nombre del evento no puede estar vacío.");
        if (!await IsEventNameUnique(newEvent.Name))
            throw new ArgumentException("El nombre del evento ya existe.");

        DateTime currentDateTime = await _dateTimeLogic.GetCurrentDateTime();
        if (newEvent.Date <= currentDateTime)
            throw new ArgumentException("La fecha del evento debe ser futura.");

        if (newEvent.Hour < MinHour || newEvent.Hour > MaxHour)
            throw new ArgumentException("La hora debe estar entre 0 y 23.");
        if (newEvent.MaxCapacity <= MinCapacityLimit || newEvent.MaxCapacity > MaxCapacityLimit)
            throw new ArgumentException(
                $"La capacidad máxima debe ser mayor a 0 y menor o igual a {MaxCapacityLimit}.");
        if (newEvent.Cost <= MinCost)
            throw new ArgumentException("El coste debe ser mayor a 0.");
    }

    private async Task<bool> IsEventNameUnique(string name)
    {
        List<Event> events = await _eventRepository.GetAll() ?? new List<Event>();
        return !events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}