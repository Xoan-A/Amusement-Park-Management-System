using Domain;

namespace IBusinessLogic
{
    public interface ITokenLogic
    {
        string GenerateToken(User user);
    }
}