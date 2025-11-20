using Domain;

namespace TestDomain;

[TestClass]
public class UserRoleTest
{
    [TestMethod]
    public void UserRole_ShouldCreateUserRole_WithValidData()
    {
        Guid userId = Guid.NewGuid();
        int roleId = 1;

        UserRole userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        Assert.AreEqual(userId, userRole.UserId);
        Assert.AreEqual(roleId, userRole.RoleId);
    }

    [TestMethod]
    public void UserRole_ShouldHaveNavigationProperties()
    {
        UserRole userRole = new UserRole();
        User user = new User { Id = Guid.NewGuid(), Name = "Test", LastName = "User", Email = "test@test.com", Password = "hash" };
        Role role = new Role { Id = 1, Name = "Administrator" };

        userRole.User = user;
        userRole.Role = role;

        Assert.AreEqual(user, userRole.User);
        Assert.AreEqual(role, userRole.Role);
    }
}
