using Domain;

namespace IDataAccess;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
}
