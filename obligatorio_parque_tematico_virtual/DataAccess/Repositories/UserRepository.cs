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

        public User? GetById(Guid id)
        {
            return _context.Users.Include(u =>
            u.VisitorReports)
            .ThenInclude(vr =>
            vr.Reports)
            .ThenInclude(r
            => r.Attraction).FirstOrDefault(u => u.Id == id);
        }

        public User? GetByIdWithRoles(Guid id)
        {
            return _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Id == id);
        }

        public User? GetByEmailWithRoles(string email)
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

        public List<User> GetTopTen()
        {
            return _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == Role.Visitor))
            .OrderByDescending(u => u.DailyScore)
            .Take(10)
            .ToList();
        }

        public void ResetScores()
        {
            List<User> users = _context.Users.ToList();
            foreach (User user in users)
            {
                user.DailyScore = 0;
            }

            _context.SaveChanges();
        }

        public void Update(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Users.Update(user);
            }

            _context.SaveChanges();
        }

        public List<User> GetAllUsers()
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();
        }
    }
}