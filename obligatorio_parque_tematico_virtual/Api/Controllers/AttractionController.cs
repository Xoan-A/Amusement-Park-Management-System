using Domain;
using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
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
    public IActionResult GetAttractions()
    {
        var attractions = _attractionService.GetAllAttractions();
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
    public IActionResult GetAttractionById(Guid id)
    {
        AttractionResponse attraction = _attractionService.GetAttractionById(id);
        return Ok(attraction);
    }

    [HttpPost]
    public IActionResult CreateAttraction([FromBody] AttractionRequest newAttraction)
    {
        Guid newId = _attractionService.CreateAttraction(newAttraction);
        
        CreateAttractionResponse response = new CreateAttractionResponse
        {
            Id = newId,
            Message = "Attraction created successfully"
        };
        return CreatedAtAction(nameof(GetAttractionById), new { id = newId }, response);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateAttraction(Guid id, [FromBody] AttractionRequest updatedAttraction)
    {
        _attractionService.UpdateAttraction(id, updatedAttraction);
        MessageResponse response = new MessageResponse
        {
            Message = "Attraction updated successfully"
        };
        return Ok(response);
    }
}