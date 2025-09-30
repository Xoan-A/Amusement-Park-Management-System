using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;

namespace Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetEvents()
    {
        List<EventResponse> events = await _eventService.GetAllEvents();
        
        return Ok(events);
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        EventResponse eventResponse = await _eventService.GetEventById(id);
        return Ok(eventResponse);
    }
    
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateEvent([FromBody] EventRequest newEvent)
    {
        Guid newId = await _eventService.CreateEvent(newEvent);

        CreateEventResponse response = new CreateEventResponse
        {
            Id = newId,
            Message = "Event created successfully"
        };

        return CreatedAtAction(nameof(GetEventById), new { id = newId }, response);
    }
}