using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class RewardRepository : IRewardRepository
    {
        private readonly AppDbContext _context;

        public RewardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Reward reward)
        {
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Reward>> GetAllAsync()
        {
            return await _context.Rewards.ToListAsync();
        }

        public async Task<Reward?> GetByIdAsync(Guid id)
        {
            return await _context.Rewards.FindAsync(id);
        }

        public async Task UpdateAsync(Reward reward)
        {
            _context.Rewards.Update(reward);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            Reward? reward = await GetByIdAsync(id);
            if (reward != null)
            {
                _context.Rewards.Remove(reward);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Reward>> GetAvailableRewardsAsync()
        {
            return await _context.Rewards
                .Where(r => r.AvailableQuantity > 0)
                .ToListAsync();
        }

        public async Task<List<Reward>> GetRewardsByMembershipLevelAsync(MembershipLevel? level)
        {
            return await _context.Rewards
                .Where(r => r.RequiredMembershipLevel == level)
                .ToListAsync();
        }

        public async Task<Reward?> GetByNameAsync(string name)
        {
            return await _context.Rewards
                .FirstOrDefaultAsync(r => r.Name == name);
        }
    }
}