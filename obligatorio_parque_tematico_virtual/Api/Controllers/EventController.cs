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
    private readonly IEventLogic _eventLogic;

    public EventController(IEventLogic eventLogic)
    {
        _eventLogic = eventLogic;
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetEvents()
    {
        List<EventResponse> events = _eventLogic.GetAllEvents();

        return Ok(events);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetEventById(Guid id)
    {
        EventResponse eventResponse = _eventLogic.GetEventById(id);
        return Ok(eventResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateEvent([FromBody] EventRequest newEvent)
    {
        Guid newId = _eventLogic.CreateEvent(newEvent);

        CreateEventResponse response = new CreateEventResponse
        {
            Id = newId,
            Message = "Event created successfully"
        };

        return CreatedAtAction(nameof(GetEventById), new { id = newId }, response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public IActionResult DeleteEventById(Guid id)
    {
        _eventLogic.DeleteEvent(id);
        return NoContent();
    }
}