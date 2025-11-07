using Domain;

namespace IDataAccess;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByNameAsync(string name);
}
