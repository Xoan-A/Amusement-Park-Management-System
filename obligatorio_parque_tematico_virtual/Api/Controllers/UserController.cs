using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

    [HttpGet("{userId}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        UserResponse user = await _userLogic.GetUserResponseById(userId);
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        UserResponse user = await _userLogic.CreateUser(request);
        return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
    }

    [HttpPut("{userId}/roles")]
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

    [HttpPut("{userId}")]
    [Authorize]
    public async Task<IActionResult> ModifyUser(Guid userId, [FromBody] ModifyUserRequest request)
    {
        string actorSubClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? string.Empty;
        UserResponse updated = await _userLogic.ModifyUser(userId, actorSubClaim, request);
        return Ok(updated);
    }
}