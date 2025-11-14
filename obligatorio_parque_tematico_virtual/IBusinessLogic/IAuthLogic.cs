using Domain;
using Models.Out;

namespace IBusinessLogic
{
    public interface IAuthLogic
    {
        UserResponse Login(string email, string password);
    }
}