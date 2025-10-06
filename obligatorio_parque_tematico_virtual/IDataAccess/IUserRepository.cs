using System;
using Domain;

namespace IDataAccess
{
    public interface IUserRepository
    {
        User Create(User user);
        User GetByEmail(string email);
        User GetById(Guid id);
        User GetByIdWithRoles(Guid id);
        User GetByEmailWithRoles(string email);
        bool IsEmailUnique(string email);
        Task<List<User>> GetTopTen();
    }
}