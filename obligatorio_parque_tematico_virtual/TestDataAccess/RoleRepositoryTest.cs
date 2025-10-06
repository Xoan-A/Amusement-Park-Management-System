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
        var options = new DbContextOptionsBuilder<AppDbContext>()
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
    public void GetAll_ShouldReturnAllRoles()
    {
        // Arrange - Seed data should be in context

        // Act
        List<Role> roles = _repository.GetAll();

        // Assert
        Assert.IsNotNull(roles);
        Assert.AreEqual(3, roles.Count);
    }

    [TestMethod]
    public void GetByName_ShouldReturnRole_WhenRoleExists()
    {
        // Act
        Role? role = _repository.GetByName(Role.ADMINISTRATOR);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual(Role.ADMINISTRATOR, role.Name);
    }

    [TestMethod]
    public void GetByName_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        // Act
        Role? role = _repository.GetByName("NonExistentRole");

        // Assert
        Assert.IsNull(role);
    }

    [TestMethod]
    public void SeedData_ShouldContainAdministratorRole()
    {
        // Act
        Role? role = _repository.GetByName(Role.ADMINISTRATOR);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual("Administrator", role.Name);
    }

    [TestMethod]
    public void SeedData_ShouldContainOperatorRole()
    {
        // Act
        Role? role = _repository.GetByName(Role.OPERATOR);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual("Operator", role.Name);
    }

    [TestMethod]
    public void SeedData_ShouldContainVisitorRole()
    {
        // Act
        Role? role = _repository.GetByName(Role.VISITOR);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreEqual("Visitor", role.Name);
    }
}
