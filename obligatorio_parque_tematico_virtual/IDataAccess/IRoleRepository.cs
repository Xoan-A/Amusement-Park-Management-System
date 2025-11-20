using Domain;

namespace IDataAccess;

public interface IRoleRepository
{
    Role? GetByName(string name);
}
