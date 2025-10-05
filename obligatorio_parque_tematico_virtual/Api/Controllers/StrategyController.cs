using BusinessLogic;
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

    public StrategyController(IActiveStrategy activeStrategy)
    {
        _activeStrategy = activeStrategy;
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetStrategy()
    {
        IContreteStrategy strategy = _activeStrategy.GetStrategy();

        StrategyResponse response = new StrategyResponse
        {
            Name = strategy.Name
        };

        return Ok(response);
    }

    [HttpPut("set")]
    [Authorize(Roles = "Administrator")]
    public IActionResult SetStrategy([FromBody] SetStrategyRequest setStrategyRequest)
    {
        _activeStrategy.SetStrategy(setStrategyRequest);

        MessageResponse response = new MessageResponse
        {
            Message = "Strategy setted successfully"
        };

        return Ok(response);
    }
}