using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;
using Domain;
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
        RedemptionHistory redemption = _redemptionLogic.RedeemReward(visitorId, redeemRequest.RewardId);

        RedemptionHistoryModelOut response = MapToModelOut(redemption);

        return CreatedAtAction(nameof(GetMyRedemptionHistory), null, response);
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Visitor")]
    public IActionResult GetMyRedemptionHistory([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        Guid visitorId = GetCurrentUserId();

        List<RedemptionHistory> history;

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            history = _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom.Value, dateTo.Value);
        }
        else
        {
            history = _redemptionLogic.GetRedemptionHistory(visitorId);
        }

        List<RedemptionHistoryModelOut> response = history.Select(h => MapToModelOut(h)).ToList();

        return Ok(response);
    }

    [HttpGet("visitor/{visitorId}/history")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetVisitorRedemptionHistory(Guid visitorId, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        List<RedemptionHistory> history;

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            history = _redemptionLogic.GetRedemptionHistoryWithDateRange(visitorId, dateFrom.Value, dateTo.Value);
        }
        else
        {
            history = _redemptionLogic.GetRedemptionHistory(visitorId);
        }

        List<RedemptionHistoryModelOut> response = history.Select(h => MapToModelOut(h)).ToList();

        return Ok(response);
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

    private RedemptionHistoryModelOut MapToModelOut(RedemptionHistory redemption)
    {
        return new RedemptionHistoryModelOut
        {
            Id = redemption.Id,
            VisitorId = redemption.VisitorId,
            RewardId = redemption.RewardId,
            RedeemedAt = redemption.RedeemedAt,
            PointsSpent = redemption.PointsSpent,
            RewardName = redemption.Reward?.Name,
            VisitorName = redemption.Visitor != null ? $"{redemption.Visitor.Name} {redemption.Visitor.LastName}" : null
        };
    }
}
