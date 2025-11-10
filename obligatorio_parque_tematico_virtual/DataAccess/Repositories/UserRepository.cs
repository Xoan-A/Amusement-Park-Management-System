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

        public async Task<User> Create(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public Task<User?> GetById(Guid id)
        {
            return _context.Users.Include(u =>
                    u.VisitorReports)
                .ThenInclude(vr =>
                    vr.Reports)
                .ThenInclude(r
                    => r.Attraction).FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<User?> GetByIdWithRoles(Guid id)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public Task<User?> GetByEmailWithRoles(string email)
        {
            return _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> IsEmailUnique(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public Task<List<User>> GetTopTen()
        {
            return _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == Role.VISITOR))
                .OrderByDescending(u => u.DailyScore)
                .Take(10)
                .ToListAsync();
        }

        public async Task ResetScores()
        {
            List<User> users = await _context.Users.ToListAsync();
            foreach (User user in users)
            {
                user.DailyScore = 0;
            }

            await _context.SaveChangesAsync();
        }

        public async Task Update(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Users.Update(user);
            }
            await _context.SaveChangesAsync();
        }
    }
}