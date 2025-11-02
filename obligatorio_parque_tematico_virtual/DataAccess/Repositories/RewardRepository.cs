using DataAccess.Context;
using Domain;
using IDataAccess;

namespace DataAccess.Repositories
{
    public class RewardRepository : IRewardRepository
    {
        private readonly AppDbContext _context;

        public RewardRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(Reward reward)
        {
            _context.Rewards.Add(reward);
        }

        public List<Reward> GetAll()
        {
            return _context.Rewards.ToList();
        }

        public Reward? GetById(Guid id)
        {
            return _context.Rewards.Find(id);
        }

        public void Update(Reward reward)
        {
            _context.Rewards.Update(reward);
        }

        public void Delete(Guid id)
        {
            var reward = GetById(id);
            if (reward != null)
            {
                _context.Rewards.Remove(reward);
            }
        }

        public List<Reward> GetAvailableRewards()
        {
            return _context.Rewards
                .Where(r => r.AvailableQuantity > 0)
                .ToList();
        }

        public List<Reward> GetRewardsByMembershipLevel(MembershipLevel? level)
        {
            return _context.Rewards
                .Where(r => r.RequiredMembershipLevel == level)
                .ToList();
        }

        public Reward? GetByName(string name)
        {
            return _context.Rewards
                .FirstOrDefault(r => r.Name == name);
        }
    }
}
