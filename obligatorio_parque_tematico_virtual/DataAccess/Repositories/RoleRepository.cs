using DataAccess.Context;
using Domain;
using IDataAccess;

namespace DataAccess.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Role> GetAll()
    {
        return _context.Roles.ToList();
    }

    public Role? GetByName(string name)
    {
        return _context.Roles.FirstOrDefault(r => r.Name == name);
    }
}
