using Domain;

namespace IBusinessLogic
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}