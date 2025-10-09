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

        public Task<User?> GetByEmail(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
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
                .OrderByDescending(u => u.Score)
                .Take(10)
                .ToListAsync();
        }

        public async Task ResetScores()
        {
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                user.Score = 0;
            }

            await _context.SaveChangesAsync();
        }

        public async Task Update(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Users.Update(user);
            }

            // Fix: Newly created Reports and VisitorReports have non-default Ids (from Guid.NewGuid())
            // EF Core incorrectly marks them as Modified instead of Added
            // Check if they exist in DB and correct their state
            foreach (var entry in _context.ChangeTracker.Entries<Report>())
            {
                if (entry.State == EntityState.Modified)
                {
                    bool exists = await _context.Reports
                        .AsNoTracking()
                        .AnyAsync(r => r.Id == entry.Entity.Id);
                    if (!exists)
                    {
                        entry.State = EntityState.Added;
                    }
                }
            }

            foreach (var entry in _context.ChangeTracker.Entries<VisitorReport>())
            {
                if (entry.State == EntityState.Modified)
                {
                    bool exists = await _context.VisitorReports
                        .AsNoTracking()
                        .AnyAsync(vr => vr.Id == entry.Entity.Id);
                    if (!exists)
                    {
                        entry.State = EntityState.Added;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}