using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserLogic _userLogic;

    public UserController(IUserLogic userLogic)
    {
        _userLogic = userLogic;
    }

    [HttpPost("{userId}/roles")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddRoleToUser(Guid userId, [FromBody] AddRolesRequest request)
    {
        await _userLogic.AddRoleToUser(userId, request.Role);

        MessageResponse response = new MessageResponse
        {
            Message = $"Role '{request.Role}' added to user successfully"
        };

        return Ok(response);
    }
}