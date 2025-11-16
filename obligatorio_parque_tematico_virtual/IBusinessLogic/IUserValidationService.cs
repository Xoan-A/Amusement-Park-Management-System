namespace IBusinessLogic
{
    public interface IUserValidationService
    {
        bool ValidateEmail(string email);
        void ValidateBirthDate(DateTime birthDate);
        void ValidateRequiredFields(string name, string lastName, string email, string password);
        void ValidateEmailUniqueness(string email);
        void ValidateMembershipLevel(string membershipLevel);
    }
}
