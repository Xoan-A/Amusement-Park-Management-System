using BusinessLogic;
using IBusinessLogic;
using IBusinessLogic.Strategy;
using IDataAccess;
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
    public IActionResult GetStrategy([FromBody] GetStrategyRequest getStrategyRequest)
    {
        IContreteStrategy strategy = _activeStrategy.GetStrategy(getStrategyRequest.CurrentDate);

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

    [HttpGet("topTen")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetTopTen()
    {
        TopTenResponse response = await _userLogic.GetTopTenUsers();
        return Ok(response);
    }
}