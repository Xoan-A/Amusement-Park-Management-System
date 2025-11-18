using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IBusinessLogic;
using Models.Out;

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
        List<PluginInfoResponse> plugins = _pluginLoader.LoadPlugins();
        return Ok(plugins);
    }

    [HttpPost]
    public IActionResult AddPlugin(IFormFile dllFile)
    {
        using (Stream stream = dllFile.OpenReadStream())
        {
            _pluginLoader.AddPlugin(stream, dllFile.FileName);
        }
        return Ok();
    }
}