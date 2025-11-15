using AutoMapper;
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
        private readonly IMapper _mapper;

        public AuthLogic(IUserRepository userRepository, IPasswordLogic passwordLogic, IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordLogic = passwordLogic;
            _mapper = mapper;
        }

        public UserResponse Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Email and password must be provided.");

            User user = _userRepository.GetByEmailWithRoles(email);
            if (user == null)
                throw new ArgumentException("Invalid email or password.");

            bool isPasswordValid = _passwordLogic.VerifyPassword(password, user.Password);
            if (!isPasswordValid)
                throw new ArgumentException("Invalid email or password.");

            return _mapper.Map<UserResponse>(user);
        }
    }
}