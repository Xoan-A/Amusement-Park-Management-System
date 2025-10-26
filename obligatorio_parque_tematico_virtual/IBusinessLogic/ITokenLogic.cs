using Domain;
using Models.Out;

namespace IBusinessLogic
{
    public interface ITokenLogic
    {
        string GenerateToken(UserResponse user);
    }
}