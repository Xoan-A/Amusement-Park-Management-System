using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;

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
    public async Task<IActionResult> GetAllRewards()
    {
        List<RewardModelOut> rewards = await _rewardLogic.GetAllRewards();
        return Ok(rewards);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetRewardById(Guid id)
    {
        RewardModelOut reward = await _rewardLogic.GetRewardById(id);
        return Ok(reward);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateReward([FromBody] RewardModelIn rewardModelIn)
    {
        RewardModelOut createdReward = await _rewardLogic.CreateReward(rewardModelIn);
        return CreatedAtAction(nameof(GetRewardById), new { id = createdReward.Id }, createdReward);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateReward(Guid id, [FromBody] RewardModelIn rewardModelIn)
    {
        RewardModelOut updatedReward = await _rewardLogic.UpdateReward(id, rewardModelIn);
        return Ok(updatedReward);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteReward(Guid id)
    {
        await _rewardLogic.DeleteReward(id);
        return NoContent();
    }

    [HttpGet("available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableRewards()
    {
        List<RewardModelOut> rewards = await _rewardLogic.GetAvailableRewards();
        return Ok(rewards);
    }
}