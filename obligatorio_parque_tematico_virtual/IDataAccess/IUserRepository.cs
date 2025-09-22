using System;
using Domain;

namespace IDataAccess
{
    public interface IUserRepository
    {
        User Create(User user);
        User GetByEmail(string email);
        User GetById(Guid id);
        bool IsEmailUnique(string email);
    }
}