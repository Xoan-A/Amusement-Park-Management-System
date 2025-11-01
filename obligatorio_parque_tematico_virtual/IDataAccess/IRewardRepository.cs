using Domain;

namespace IDataAccess
{
    public interface IRewardRepository
    {
        void Create(Reward reward);
        List<Reward> GetAll();
        Reward? GetById(Guid id);
        void Update(Reward reward);
        void Delete(Guid id);
        List<Reward> GetAvailableRewards();
        List<Reward> GetRewardsByMembershipLevel(MembershipLevel? level);
        Reward? GetByName(string name);
    }
}
