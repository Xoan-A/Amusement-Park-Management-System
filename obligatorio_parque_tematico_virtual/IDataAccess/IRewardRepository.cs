using Domain;

namespace IDataAccess
{
    public interface IRewardRepository
    {
        Task CreateAsync(Reward reward);
        Task<List<Reward>> GetAllAsync();
        Task<Reward?> GetByIdAsync(Guid id);
        Task UpdateAsync(Reward reward);
        Task DeleteAsync(Guid id);
        Task<List<Reward>> GetAvailableRewardsAsync();
        Task<Reward?> GetByNameAsync(string name);
    }
}
