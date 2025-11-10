using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IBusinessLogic;
using System.Security.Claims;
using Models.Out;

namespace Api.Controllers;

[ApiController]
[Route("api/score-history")]
public class ScoreHistoryController : ControllerBase
{
    private readonly IScoreHistoryLogic _scoreHistoryLogic;
    private readonly IClaimsLogic _claimsLogic;

    public ScoreHistoryController(IScoreHistoryLogic scoreHistoryLogic, IClaimsLogic claimsLogic)
    {
        _scoreHistoryLogic = scoreHistoryLogic;
        _claimsLogic = claimsLogic;
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> GetMyScoreHistory()
    {
        Guid userId = _claimsLogic.GetCurrentUserId(User);
        List<ScoreHistoryModelOut> history = await _scoreHistoryLogic.GetMyScoreHistory(userId);
        return Ok(history);
    }

    [HttpGet("visitor/{visitorId}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetVisitorHistory(Guid visitorId, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        List<ScoreHistoryModelOut> history = await _scoreHistoryLogic.GetVisitorScoreHistory(visitorId, dateFrom, dateTo);
        return Ok(history);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAllHistory()
    {
        List<ScoreHistoryModelOut> history = await _scoreHistoryLogic.GetAllScoreHistory();
        return Ok(history);
    }
}