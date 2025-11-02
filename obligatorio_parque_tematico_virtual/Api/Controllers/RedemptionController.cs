using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using System.Security.Claims;

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

    [HttpPost("redeem")]
    [Authorize(Roles = "Visitor")]
    public IActionResult RedeemReward([FromBody] RedeemRewardModelIn redeemRequest)
    {
        Guid visitorId = GetCurrentUserId();
        var redemption = _redemptionLogic.RedeemReward(visitorId, redeemRequest.RewardId);
        return CreatedAtAction(nameof(GetMyRedemptionHistory), null, redemption);
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Visitor")]
    public IActionResult GetMyRedemptionHistory([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        Guid visitorId = GetCurrentUserId();

        var history = dateFrom.HasValue && dateTo.HasValue
            ? _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom.Value, dateTo.Value)
            : _redemptionLogic.GetRedemptionHistory(visitorId);

        return Ok(history);
    }

    [HttpGet("visitor/{visitorId}/history")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetVisitorRedemptionHistory(Guid visitorId, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var history = dateFrom.HasValue && dateTo.HasValue
            ? _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom.Value, dateTo.Value)
            : _redemptionLogic.GetRedemptionHistory(visitorId);

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
