using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class AuthLogic : IAuthLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public AuthLogic(IUserRepository userRepository, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        public User Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            User user = _userRepository.GetByEmailWithRoles(email);
            if (user == null)
            {
                return null;
            }

            bool isPasswordValid = _passwordService.VerifyPassword(password, user.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }
    }
}