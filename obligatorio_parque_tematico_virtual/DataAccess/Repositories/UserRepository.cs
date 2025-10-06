using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public User Create(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User GetById(Guid id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }

        public User GetByIdWithRoles(Guid id)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Id == id);
        }

        public User GetByEmailWithRoles(string email)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Email == email);
        }

        public bool IsEmailUnique(string email)
        {
            return !_context.Users.Any(u => u.Email == email);
        }

        public Task<List<User>> GetTopTen()
        {
            return _context.Users
                .OrderByDescending(u => u.Score)
                .Take(10)
                .ToListAsync();
        }
    }
}