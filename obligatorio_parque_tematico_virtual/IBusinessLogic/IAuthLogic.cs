using Domain;
using Models.Out;

namespace IBusinessLogic
{
    public interface IAuthLogic
    {
        Task<UserResponse> Login(string email, string password);
    }
}