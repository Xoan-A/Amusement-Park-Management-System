using IBusinessLogic;
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
    private readonly IUserManagementLogic _userManagementLogic;

    public StrategyController(IActiveStrategy activeStrategy, IUserManagementLogic userManagementLogic)
    {
        _activeStrategy = activeStrategy;
        _userManagementLogic = userManagementLogic;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetStrategy()
    {
        IConcreteStrategy strategy = _activeStrategy.GetStrategy();

        StrategyResponse response = new StrategyResponse
        {
            Name = strategy.Name
        };

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = "Administrator")]
    public IActionResult SetStrategy([FromBody] SetStrategyRequest setStrategyRequest)
    {
        _activeStrategy.SetStrategy(setStrategyRequest);

        MessageResponse response = new MessageResponse
        {
            Message = "Strategy set successfully"
        };

        return Ok(response);
    }

    [HttpGet("topTen")]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetTopTen()
    {
        TopTenResponse response = _userManagementLogic.GetTopTenUsers();
        return Ok(response);
    }
}