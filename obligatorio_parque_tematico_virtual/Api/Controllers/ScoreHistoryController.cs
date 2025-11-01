using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IDataAccess;
using Domain;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/score-history")]
public class ScoreHistoryController : ControllerBase
{
    private readonly IScoreHistoryRepository _scoreHistoryRepository;

    public ScoreHistoryController(IScoreHistoryRepository scoreHistoryRepository)
    {
        _scoreHistoryRepository = scoreHistoryRepository;
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Visitor")]
    public IActionResult GetMyScoreHistory()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var history = _scoreHistoryRepository.GetByVisitor(userId);

        return Ok(history.Select(h => new
        {
            h.Id,
            h.CreatedAt,
            h.Points,
            Origin = h.Origin.ToString(),
            h.StrategyName,
            h.Description,
            h.RelatedEntityId
        }));
    }

    [HttpGet("visitor/{visitorId}")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetVisitorHistory(Guid visitorId, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        List<ScoreHistory> history;

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            history = _scoreHistoryRepository.GetByVisitorAndDateRange(visitorId, dateFrom.Value, dateTo.Value);
        }
        else
        {
            history = _scoreHistoryRepository.GetByVisitor(visitorId);
        }

        return Ok(history.Select(h => new
        {
            h.Id,
            h.CreatedAt,
            h.Points,
            Origin = h.Origin.ToString(),
            h.StrategyName,
            h.Description,
            h.RelatedEntityId
        }));
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetAllHistory()
    {
        var history = _scoreHistoryRepository.GetAll();

        return Ok(history.Select(h => new
        {
            h.Id,
            h.VisitorId,
            VisitorName = h.Visitor != null ? $"{h.Visitor.Name} {h.Visitor.LastName}" : "Unknown",
            h.CreatedAt,
            h.Points,
            Origin = h.Origin.ToString(),
            h.StrategyName,
            h.Description
        }));
    }
}
