using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Models.In;
using Models.Out;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserLogic _userLogic;
    private readonly IClaimsLogic _claimsLogic;

    public UserController(IUserLogic userLogic, IClaimsLogic claimsLogic)
    {
        _userLogic = userLogic;
        _claimsLogic = claimsLogic;
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
        Guid userTokenId = _claimsLogic.GetCurrentUserId(User);
        UserResponse updated = await _userLogic.ModifyUser(userId, userTokenId, request);
        return Ok(updated);
    }

    [HttpPut("{userId}/membership")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ChangeMembershipLevel(Guid userId, [FromBody] ChangeMembershipLevelRequest request)
    {
        UserResponse updated = await _userLogic.ChangeMembershipLevel(userId, request.MembershipLevel);
        return Ok(updated);
    }
}