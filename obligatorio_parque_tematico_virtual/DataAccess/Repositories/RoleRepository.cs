using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public Role? GetByName(string name)
    {
        return _context.Roles.FirstOrDefault(r => r.Name == name);
    }
}