using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;
using Domain;

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
        List<Reward> rewards = _rewardLogic.GetAllRewards();
        List<RewardModelOut> response = rewards.Select(r => MapToModelOut(r)).ToList();
        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetRewardById(Guid id)
    {
        Reward reward = _rewardLogic.GetRewardById(id);
        RewardModelOut response = MapToModelOut(reward);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateReward([FromBody] RewardModelIn rewardModelIn)
    {
        Reward reward = new Reward
        {
            Name = rewardModelIn.Name,
            Description = rewardModelIn.Description,
            PointsCost = rewardModelIn.PointsCost,
            AvailableQuantity = rewardModelIn.AvailableQuantity,
            RequiredMembershipLevel = rewardModelIn.RequiredMembershipLevel
        };

        Reward createdReward = _rewardLogic.CreateReward(reward);
        RewardModelOut response = MapToModelOut(createdReward);

        return CreatedAtAction(nameof(GetRewardById), new { id = createdReward.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public IActionResult UpdateReward(Guid id, [FromBody] RewardModelIn rewardModelIn)
    {
        Reward reward = new Reward
        {
            Name = rewardModelIn.Name,
            Description = rewardModelIn.Description,
            PointsCost = rewardModelIn.PointsCost,
            AvailableQuantity = rewardModelIn.AvailableQuantity,
            RequiredMembershipLevel = rewardModelIn.RequiredMembershipLevel
        };

        Reward updatedReward = _rewardLogic.UpdateReward(id, reward);
        RewardModelOut response = MapToModelOut(updatedReward);

        return Ok(response);
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
        List<Reward> rewards = _rewardLogic.GetAvailableRewards();
        List<RewardModelOut> response = rewards.Select(r => MapToModelOut(r)).ToList();
        return Ok(response);
    }

    private RewardModelOut MapToModelOut(Reward reward)
    {
        return new RewardModelOut
        {
            Id = reward.Id,
            Name = reward.Name,
            Description = reward.Description,
            PointsCost = reward.PointsCost,
            AvailableQuantity = reward.AvailableQuantity,
            RequiredMembershipLevel = reward.RequiredMembershipLevel,
            IsAvailable = reward.IsAvailable()
        };
    }
}
