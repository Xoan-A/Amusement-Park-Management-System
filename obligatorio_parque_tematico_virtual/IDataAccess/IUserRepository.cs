using Domain;

namespace IDataAccess
{
    public interface IUserRepository
    {
        Task<User> Create(User user);
        Task<User?> GetByEmail(string email);
        Task<User?> GetById(Guid id);
        Task<User?> GetByIdWithRoles(Guid id);
        Task<User?> GetByEmailWithRoles(string email);
        Task<bool> IsEmailUnique(string email);
        Task<List<User>> GetTopTen();
        Task ResetScores();
        Task Update(User user);
    }
}