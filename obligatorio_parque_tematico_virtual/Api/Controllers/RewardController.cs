using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;

namespace Api.Controllers;

[ApiController]
[Route("api/rewards")]
public class RewardController : ControllerBase
{
    private readonly IRewardLogic _rewardLogic;

    public RewardController(IRewardLogic rewardLogic)
    {
        _rewardLogic = rewardLogic;
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetAllRewards()
    {
        var rewards = _rewardLogic.GetAllRewards();
        return Ok(rewards);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetRewardById(Guid id)
    {
        var reward = _rewardLogic.GetRewardById(id);
        return Ok(reward);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateReward([FromBody] RewardModelIn rewardModelIn)
    {
        var createdReward = _rewardLogic.CreateReward(rewardModelIn);
        return CreatedAtAction(nameof(GetRewardById), new { id = createdReward.Id }, createdReward);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public IActionResult UpdateReward(Guid id, [FromBody] RewardModelIn rewardModelIn)
    {
        var updatedReward = _rewardLogic.UpdateReward(id, rewardModelIn);
        return Ok(updatedReward);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public IActionResult DeleteReward(Guid id)
    {
        _rewardLogic.DeleteReward(id);
        return NoContent();
    }

    [HttpGet("available")]
    [Authorize]
    public IActionResult GetAvailableRewards()
    {
        var rewards = _rewardLogic.GetAvailableRewards();
        return Ok(rewards);
    }
}
