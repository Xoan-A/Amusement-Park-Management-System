using Domain;
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
    private readonly IAttractionService _attractionService;

    public AttractionController(IAttractionService attractionService)
    {
        _attractionService = attractionService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAttractions()
    {
        var attractions = await _attractionService.GetAllAttractions();
        var response = new AllAttractionsResponse();

        foreach (var attraction in attractions)
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
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAttractionById(Guid id)
    {
        AttractionResponse attraction = await _attractionService.GetAttractionById(id);
        return Ok(attraction);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateAttraction([FromBody] AttractionRequest newAttraction)
    {
        Guid newId = await _attractionService.CreateAttraction(newAttraction);

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
        await _attractionService.UpdateAttraction(id, updatedAttraction);
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
        await _attractionService.DeleteAttraction(id);
        return NoContent();
    }
}