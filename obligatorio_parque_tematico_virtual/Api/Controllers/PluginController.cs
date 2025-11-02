using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IBusinessLogic;

namespace Api.Controllers;

[ApiController]
[Route("api/plugins")]
[Authorize(Roles = "Administrator")]
public class PluginController : ControllerBase
{
    private readonly IPluginLoader _pluginLoader;

    public PluginController(IPluginLoader pluginLoader)
    {
        _pluginLoader = pluginLoader;
    }

    [HttpGet]
    public IActionResult GetAvailablePlugins()
    {
        var plugins = _pluginLoader.LoadPlugins();
        return Ok(plugins);
    }

    [HttpGet("{name}")]
    public IActionResult GetPluginByName(string name)
    {
        var plugin = _pluginLoader.GetPluginByName(name);

        if (plugin == null)
        {
            return NotFound(new { Message = $"Plugin '{name}' not found" });
        }

        return Ok(plugin);
    }
}
