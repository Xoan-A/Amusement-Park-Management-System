using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace TestDataAccess;

[TestClass]
public class RoleRepositoryTest
{
    private AppDbContext _context = null!;
    private RoleRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        _repository = new RoleRepository(_context);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetByName_ShouldReturnRole_WhenRoleExists()
    {
        Role? role = await _repository.GetByNameAsync(Role.ADMINISTRATOR);

        Assert.AreEqual(Role.ADMINISTRATOR, role.Name);
    }

    [TestMethod]
    public async Task GetByName_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        Role? role = await _repository.GetByNameAsync("NonExistentRole");

        Assert.IsNull(role);
    }

    [TestMethod]
    public async Task SeedData_ShouldContainAdministratorRole()
    {
        Role? role = await _repository.GetByNameAsync(Role.ADMINISTRATOR);

        Assert.AreEqual("Administrator", role.Name);
    }

    [TestMethod]
    public async Task SeedData_ShouldContainOperatorRole()
    {
        Role? role = await _repository.GetByNameAsync(Role.OPERATOR);

        Assert.AreEqual("Operator", role.Name);
    }

    [TestMethod]
    public async Task SeedData_ShouldContainVisitorRole()
    {
        Role? role = await _repository.GetByNameAsync(Role.VISITOR);

        Assert.AreEqual("Visitor", role.Name);
    }
}
