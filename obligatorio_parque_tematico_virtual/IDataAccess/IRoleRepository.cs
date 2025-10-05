using Domain;

namespace IDataAccess;

public interface IRoleRepository
{
    List<Role> GetAll();
    Role? GetByName(string name);
}
