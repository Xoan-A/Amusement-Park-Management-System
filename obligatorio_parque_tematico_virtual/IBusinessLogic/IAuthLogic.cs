using Domain;

namespace IBusinessLogic
{
    public interface IAuthLogic
    {
        Task<User> Login(string email, string password);
    }
}