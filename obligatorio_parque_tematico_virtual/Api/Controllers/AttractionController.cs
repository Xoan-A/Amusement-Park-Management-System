using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;

namespace Api.Controllers;

[ApiController]
[Route("api/attractions")]
public class AttractionController : ControllerBase
{
    private readonly IAttractionLogic _attractionLogic;
    private readonly IUserLogic _userService;

    public AttractionController(IAttractionLogic attractionLogic, IUserLogic userService)
    {
        _attractionLogic = attractionLogic;
        _userService = userService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAttractions()
    {
        List<AttractionResponse> attractions = await _attractionLogic.GetAllAttractions();
        AllAttractionsResponse response = new AllAttractionsResponse();

        foreach (AttractionResponse attraction in attractions)
        {
            response.Attractions.Add(new AttractionResponse()
            {
                Id = attraction.Id,
                Name = attraction.Name,
                Description = attraction.Description,
                Type = attraction.Type.ToString(),
                MinAge = attraction.MinAge,
                MaxCapacity = attraction.MaxCapacity,
                CurrentCapacity = attraction.CurrentCapacity,
                IsActive = attraction.IsActive
            });
        }

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetAttractionById(Guid id)
    {
        AttractionResponse attraction = await _attractionLogic.GetAttractionById(id);
        return Ok(attraction);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateAttraction([FromBody] AttractionRequest newAttraction)
    {
        Guid newId = await _attractionLogic.CreateAttraction(newAttraction);

        CreateAttractionResponse response = new CreateAttractionResponse
        {
            Id = newId,
            Message = "Attraction created successfully"
        };
        return CreatedAtAction(nameof(GetAttractionById), new { id = newId }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateAttraction(Guid id, [FromBody] AttractionRequest updatedAttraction)
    {
        await _attractionLogic.UpdateAttraction(id, updatedAttraction);
        MessageResponse response = new MessageResponse
        {
            Message = "Attraction updated successfully"
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteAttraction(Guid id)
    {
        await _attractionLogic.DeleteAttraction(id);
        return NoContent();
    }

    [HttpPut("entry/{id}")]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> RegisterEntry(Guid id, [FromBody] RegisterEntryRequest registerEntryRequest)
    {
        await _userService.RegisterEntry(id, registerEntryRequest);

        MessageResponse response = new MessageResponse
        {
            Message = "Entry registered successfully"
        };
        return Ok(response);
    }

    [HttpPut("exit/{id}")]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> RegisterExit(Guid id, [FromBody] RegisterExitRequest registerExitRequest)
    {
        await _userService.RegisterExit(id, registerExitRequest);

        MessageResponse response = new MessageResponse
        {
            Message = "Exit registered successfully"
        };
        return Ok(response);
    }

    [HttpGet("capacity/{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetCapacity(Guid id)
    {
        CapacityResponse capacity = await _attractionLogic.GetCapacity(id);
        return Ok(capacity);
    }

    [HttpGet("visits")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAttractionsVisits([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        AttractionsVisitsRequest request = new AttractionsVisitsRequest
        {
            StartDate = startDate,
            EndDate = endDate
        };
        AttractionsVisitResponse visits = await _attractionLogic.GetAllAttractionsVisits(request);
        return Ok(visits);
    }
}