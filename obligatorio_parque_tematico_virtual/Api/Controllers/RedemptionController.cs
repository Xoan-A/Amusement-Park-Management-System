using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Models.Out;

namespace Api.Controllers;

[ApiController]
[Route("api/redemptions")]
public class RedemptionController : ControllerBase
{
    private readonly IRedemptionLogic _redemptionLogic;

    public RedemptionController(IRedemptionLogic redemptionLogic)
    {
        _redemptionLogic = redemptionLogic;
    }

    [HttpPost("redeem/{rewardId}")]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> RedeemReward(Guid rewardId)
    {
        Guid visitorId = GetCurrentUserId();
        RedemptionHistoryModelOut redemption = await _redemptionLogic.RedeemReward(visitorId, rewardId);
        return CreatedAtAction(nameof(GetMyRedemptionHistory), null, redemption);
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> GetMyRedemptionHistory([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        Guid visitorId = GetCurrentUserId();

        List<RedemptionHistoryModelOut> history = dateFrom.HasValue && dateTo.HasValue
            ? await _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom.Value, dateTo.Value)
            : await _redemptionLogic.GetRedemptionHistory(visitorId);

        return Ok(history);
    }

    [HttpGet("visitor/{visitorId}/history")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetVisitorRedemptionHistory(Guid visitorId, [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo)
    {
        List<RedemptionHistoryModelOut> history = dateFrom.HasValue && dateTo.HasValue
            ? await _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom.Value, dateTo.Value)
            : await _redemptionLogic.GetRedemptionHistory(visitorId);

        return Ok(history);
    }

    private Guid GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return Guid.Parse(userIdClaim);
    }
}