using AutoMapper;
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
    private readonly IMapper _mapper;
    private const int MinCapacityLimit = 1;
    private const int MaxCapacityLimit = 10000;
    private const int MinHour = 0;
    private const int MaxHour = 23;
    private const int MinCost = 1;

    public EventLogic(IEventRepository eventRepository, IAttractionLogicEntity attractionLogic,
        IDateTimeLogic dateTimeLogic, IMapper mapper)
    {
        _eventRepository = eventRepository;
        _attractionLogic = attractionLogic;
        _dateTimeLogic = dateTimeLogic;
        _mapper = mapper;
    }

    public EventResponse GetEventById(Guid expectedEventId)
    {
        Event eventEntity = _eventRepository.GetById(expectedEventId);
        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con id {expectedEventId}");
        }

        return _mapper.Map<EventResponse>(eventEntity);
    }

    public List<EventResponse> GetAllEvents()
    {
        List<Event> events = _eventRepository.GetAll();
        return _mapper.Map<List<EventResponse>>(events);
    }

    public Guid CreateEvent(EventRequest newEvent)
    {
        ValidateEventRequest(newEvent);
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
                Attraction attraction = _attractionLogic.GetAttractionEntityById(attractionId);
                if (attraction == null)
                {
                    throw new KeyNotFoundException($"No se encontró la atracción con id {attractionId}");
                }

                eventEntity.AddAttraction(attraction);
            }
        }

        _eventRepository.Create(eventEntity);
        return eventEntity.Id;
    }

    public void DeleteEvent(Guid eventId)
    {
        Event eventEntity = _eventRepository.GetById(eventId);
        if (eventEntity == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con id {eventId}");
        }

        _eventRepository.Delete(eventEntity);
    }

    private void ValidateEventRequest(EventRequest newEvent)
    {
        if (string.IsNullOrWhiteSpace(newEvent.Name))
            throw new ArgumentException("El nombre del evento no puede estar vacío.");
        if (!IsEventNameUnique(newEvent.Name))
            throw new ArgumentException("El nombre del evento ya existe.");

        DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
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

    private bool IsEventNameUnique(string name)
    {
        List<Event> events = _eventRepository.GetAll() ?? new List<Event>();
        return !events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}