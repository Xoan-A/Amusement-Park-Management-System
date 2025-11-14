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
    public IActionResult GetAttractionIncidents(Guid id)
    {
        List<string> incidents = _attractionLogic.GetAttractionIncidents(id);
        return Ok(incidents);
    }

    [HttpPut]
    [Authorize(Roles = "Operator")]
    public IActionResult AddIncident(Guid id, [FromBody] IncidentRequest request)
    {
        _attractionLogic.AddIncident(id, request.Incident);
        return Ok(new { Message = "Incident reported successfully" });
    }

    [HttpDelete]
    [Authorize(Roles = "Operator")]
    public IActionResult RemoveIncident(Guid id, [FromQuery] IncidentRequest request)
    {
        _attractionLogic.RemoveIncident(id, request.Incident);
        return NoContent();
    }
}