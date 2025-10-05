using Domain;

namespace IBusinessLogic
{
    public interface IAuthLogic
    {
        User Login(string email, string password);
    }
}