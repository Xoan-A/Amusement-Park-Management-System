using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class UserValidationService : IUserValidationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeLogic _dateTimeLogic;

        public UserValidationService(IUserRepository userRepository, IDateTimeLogic dateTimeLogic)
        {
            _userRepository = userRepository;
            _dateTimeLogic = dateTimeLogic;
        }

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!email.Contains("@"))
                return false;

            int atIndex = email.IndexOf("@");
            if (atIndex == 0 || atIndex == email.Length - 1)
                return false;

            if (email.IndexOf("@", atIndex + 1) != -1)
                return false;

            string domain = email.Substring(atIndex + 1);
            if (!domain.Contains("."))
                return false;

            if (domain.StartsWith(".") || domain.EndsWith("."))
                return false;

            return true;
        }

        public void ValidateBirthDate(DateTime birthDate)
        {
            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
            if (birthDate >= currentDateTime)
                throw new ArgumentException("Birth date cannot be after today.");
        }

        public void ValidateRequiredFields(string name, string lastName, string email, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Name, last name, email, and password must be provided.");
        }

        public void ValidateEmailUniqueness(string email)
        {
            if (!_userRepository.IsEmailUnique(email))
                throw new ArgumentException("Email is already in use.");
        }

        public void ValidateMembershipLevel(int membershipLevel)
        {
            if (!Enum.IsDefined(typeof(MembershipLevel), membershipLevel))
            {
                throw new ArgumentException("Invalid membership level.");
            }
        }
    }
}
