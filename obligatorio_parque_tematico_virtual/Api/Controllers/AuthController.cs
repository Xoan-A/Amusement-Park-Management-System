using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Models.In;
using Models.Out;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthLogic _authLogic;
        private readonly IUserLogic _userLogic;

        public AuthController(IAuthLogic authLogic, IUserLogic userLogic)
        {
            _authLogic = authLogic;
            _userLogic = userLogic;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var token = _authLogic.Login(request.Email, request.Password);

            var response = new LoginResponse
            {
                Token = token,
                Email = request.Email,
                Role = "User",
                Name = "User Name"
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterVisitorRequest request)
        {
            var visitor = _userLogic.RegisterVisitor(request.Name, request.LastName, request.Email, request.Password, request.BirthDate);

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