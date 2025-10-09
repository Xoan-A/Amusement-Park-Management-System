using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.Out;

namespace BusinessLogic
{
    public class AuthLogic : IAuthLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordLogic _passwordLogic;

        public AuthLogic(IUserRepository userRepository, IPasswordLogic passwordLogic)
        {
            _userRepository = userRepository;
            _passwordLogic = passwordLogic;
        }

        public async Task<UserResponse> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Email and password must be provided.");

            User user = await _userRepository.GetByEmailWithRoles(email);
            if (user == null)
                throw new ArgumentException("Invalid email or password.");

            bool isPasswordValid = _passwordLogic.VerifyPassword(password, user.Password);
            if (!isPasswordValid)
                throw new ArgumentException("Invalid email or password.");
            
            UserResponse userResponse = new UserResponse()
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                LastName = user.LastName,
                UserRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                BirthDate = user.BirthDate,
                MembershipLevel = (int?)user.MembershipLevel,
                Score = user.Score
            };

            return userResponse;
        }
    }
}