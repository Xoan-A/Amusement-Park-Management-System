using IBusinessLogic;
using IBusinessLogic.Strategy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.In;
using Models.Out;

namespace Api.Controllers;

[ApiController]
[Route("api/strategy")]
public class StrategyController : ControllerBase
{
    private readonly IActiveStrategy _activeStrategy;
    private readonly IUserLogic _userLogic;

    public StrategyController(IActiveStrategy activeStrategy, IUserLogic userLogic)
    {
        _activeStrategy = activeStrategy;
        _userLogic = userLogic;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetStrategy()
    {
        IConcreteStrategy strategy = await _activeStrategy.GetStrategy();

        StrategyResponse response = new StrategyResponse
        {
            Name = strategy.Name
        };

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SetStrategy([FromBody] SetStrategyRequest setStrategyRequest)
    {
        await _activeStrategy.SetStrategy(setStrategyRequest);

        MessageResponse response = new MessageResponse
        {
            Message = "Strategy set successfully"
        };

        return Ok(response);
    }

    [HttpGet("topTen")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetTopTen()
    {
        TopTenResponse response = await _userLogic.GetTopTenUsers();
        return Ok(response);
    }
}