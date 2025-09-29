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
}