using System.Linq;
using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Domain.Exceptions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthLogic _authLogic;
        private readonly IUserLogic _userLogic;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthLogic authLogic, IUserLogic userLogic, ITokenService tokenService)
        {
            _authLogic = authLogic;
            _userLogic = userLogic;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _authLogic.Login(request.Email, request.Password);

            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            var token = _tokenService.GenerateToken(user);
            var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToArray() ?? new string[0];

            var response = new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Roles = roles,
                Name = $"{user.Name} {user.LastName}"
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterVisitorRequest request)
        {
            var visitor = _userLogic.RegisterVisitor(request.Name, request.LastName, request.Email, request.Password, request.BirthDate);

            if (visitor == null)
            {
                return BadRequest(new { Message = "Registration failed" });
            }

            var response = new RegisterResponse
            {
                Id = visitor.Id,
                Email = visitor.Email,
                Message = "Registration successful"
            };

            return Ok(response);
        }
    }
}