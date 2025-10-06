using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;

namespace Api.Controllers;

[ApiController]
[Route("api/incidents/{id}")]
public class IncidentController : ControllerBase
{
    private readonly IAttractionLogic _attractionLogic;

    public IncidentController(IAttractionLogic attractionLogic)
    {
        _attractionLogic = attractionLogic;
    }

    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> GetAttractionIncidents(Guid id)
    {
        var incidents = await _attractionLogic.GetAttractionIncidents(id);
        return Ok(incidents);
    }

    [HttpPost]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> AddIncident(Guid id, [FromBody] IncidentRequest request)
    {
        await _attractionLogic.AddIncident(id, request.Incident);
        return Ok(new { Message = "Incident reported successfully" });
    }

    [HttpDelete]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> RemoveIncident(Guid id, [FromBody] IncidentRequest request)
    {
        await _attractionLogic.RemoveIncident(id, request.Incident);
        return Ok(new { Message = "Incident resolved successfully" });
    }
}