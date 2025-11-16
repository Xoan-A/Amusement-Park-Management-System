using Domain;

namespace IDataAccess
{
    public interface IUserRepository
    {
        User Create(User user);
        User? GetById(Guid id);
        User? GetByIdWithRoles(Guid id);
        User? GetByEmailWithRoles(string email);
        bool IsEmailUnique(string email);
        List<User> GetTopTen();
        void ResetScores();
        void Update(User user);
        List<User> GetAllUsers();
    }
}