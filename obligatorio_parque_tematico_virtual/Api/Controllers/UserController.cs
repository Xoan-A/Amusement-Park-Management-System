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
    private readonly IUserManagementLogic _userManagementLogic;
    private readonly IClaimsLogic _claimsLogic;

    public UserController(IUserManagementLogic userManagementLogic, IClaimsLogic claimsLogic)
    {
        _userManagementLogic = userManagementLogic;
        _claimsLogic = claimsLogic;
    }

    [HttpGet("{userId}")]
    [Authorize]
    public IActionResult GetUserById(Guid userId)
    {
        Guid currentUserId = _claimsLogic.GetCurrentUserId(User);
        bool isAdmin = User.IsInRole("Administrator");

        UserResponse user = _userManagementLogic.GetUserResponseById(userId, currentUserId, isAdmin);
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateUser([FromBody] CreateUserRequest request)
    {
        UserResponse user = _userManagementLogic.CreateUser(request);
        return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
    }

    [HttpPut("{userId}/roles")]
    [Authorize(Roles = "Administrator")]
    public IActionResult AddRoleToUser(Guid userId, [FromBody] AddRolesRequest request)
    {
        _userManagementLogic.AddRoleToUser(userId, request.Role);

        MessageResponse response = new MessageResponse
        {
            Message = $"Role '{request.Role}' added to user successfully"
        };

        return Ok(response);
    }

    [HttpPut("{userId}")]
    [Authorize]
    public IActionResult ModifyUser(Guid userId, [FromBody] ModifyUserRequest request)
    {
        Guid userTokenId = _claimsLogic.GetCurrentUserId(User);
        UserResponse updated = _userManagementLogic.ModifyUser(userId, userTokenId, request);
        return Ok(updated);
    }

    [HttpPut("{userId}/membership")]
    [Authorize(Roles = "Administrator")]
    public IActionResult ChangeMembershipLevel(Guid userId, [FromBody] ChangeMembershipLevelRequest request)
    {
        UserResponse updated = _userManagementLogic.ChangeMembershipLevel(userId, request.MembershipLevel);
        return Ok(updated);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public IActionResult GetAllUsers()
    {
        List<UserResponse> users = _userManagementLogic.GetAllUsers();
        return Ok(users);
    }
}