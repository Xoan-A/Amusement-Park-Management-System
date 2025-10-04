using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;

namespace Api.Controllers;

[ApiController]
[Route("api/incidents/{id}")]
public class IncidentController : ControllerBase
{
    private readonly IAttractionService _attractionService;

    public IncidentController(IAttractionService attractionService)
    {
        _attractionService = attractionService;
    }

    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> GetAttractionIncidents(Guid id)
    {
        var incidents = await _attractionService.GetAttractionIncidents(id);
        return Ok(incidents);
    }

    [HttpPost]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> AddIncident(Guid id, [FromBody] IncidentRequest request)
    {
        await _attractionService.AddIncident(id, request.Incident);
        return Ok(new { Message = "Incident reported successfully" });
    }

    [HttpDelete]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> RemoveIncident(Guid id, [FromBody] IncidentRequest request)
    {
        await _attractionService.RemoveIncident(id, request.Incident);
        return Ok(new { Message = "Incident resolved successfully" });
    }
}