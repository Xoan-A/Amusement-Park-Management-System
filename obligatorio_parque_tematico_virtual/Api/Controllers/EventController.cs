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
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetEvents()
    {
        List<EventResponse> events = await _eventLogic.GetAllEvents();

        return Ok(events);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        EventResponse eventResponse = await _eventLogic.GetEventById(id);
        return Ok(eventResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateEvent([FromBody] EventRequest newEvent)
    {
        Guid newId = await _eventLogic.CreateEvent(newEvent);

        CreateEventResponse response = new CreateEventResponse
        {
            Id = newId,
            Message = "Event created successfully"
        };

        return CreatedAtAction(nameof(GetEventById), new { id = newId }, response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteEventById(Guid id)
    {
        await _eventLogic.DeleteEvent(id);
        return NoContent();
    }
}