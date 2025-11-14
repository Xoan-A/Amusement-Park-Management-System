using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Models.In;
using Models.Out;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthLogic _authLogic;
        private readonly IUserLogic _userLogic;
        private readonly ITokenLogic _tokenLogic;

        public AuthController(IAuthLogic authLogic, IUserLogic userLogic, ITokenLogic tokenLogic)
        {
            _authLogic = authLogic;
            _userLogic = userLogic;
            _tokenLogic = tokenLogic;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            UserResponse user = _authLogic.Login(request.Email, request.Password);

            string token = _tokenLogic.GenerateToken(user);
            string[] roles = user.UserRoles?.ToArray() ?? new string[0];

            LoginResponse response = new LoginResponse
            {
                Token = token,
                Id = user.Id,
                Email = user.Email,
                Roles = roles,
                Name = $"{user.Name} {user.LastName}"
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterVisitorRequest request)
        {
            UserResponse visitor = _userLogic.RegisterVisitor(request);

            RegisterResponse response = new RegisterResponse
            {
                Id = visitor.Id,
                Email = visitor.Email,
                Message = "Registration successful"
            };

            return Ok(response);
        }
    }
}