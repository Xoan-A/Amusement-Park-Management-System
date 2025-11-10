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
    private readonly IClaimsLogic _claimsLogic;

    public RedemptionController(IRedemptionLogic redemptionLogic, IClaimsLogic claimsLogic)
    {
        _redemptionLogic = redemptionLogic;
        _claimsLogic = claimsLogic;
    }

    [HttpPost("redeem/{rewardId}")]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> RedeemReward(Guid rewardId)
    {
        Guid visitorId = _claimsLogic.GetCurrentUserId(User);
        RedemptionHistoryModelOut redemption = await _redemptionLogic.RedeemReward(visitorId, rewardId);
        return CreatedAtAction(nameof(GetMyRedemptionHistory), null, redemption);
    }

    [HttpGet("my-history")]
    [Authorize(Roles = "Visitor")]
    public async Task<IActionResult> GetMyRedemptionHistory([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        Guid visitorId = _claimsLogic.GetCurrentUserId(User);
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
}