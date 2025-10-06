using Domain;
using IBusinessLogic;
using IDataAccess;

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

            bool isPasswordValid = _passwordLogic.VerifyPassword(password, user.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            return user;
        }
    }
}