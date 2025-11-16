namespace IBusinessLogic
{
    public interface IPasswordLogic
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        bool ValidatePassword(string password);
    }
}