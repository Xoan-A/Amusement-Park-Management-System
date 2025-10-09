using Domain;

namespace TestDomain;

[TestClass]
public class RoleTest
{
    [TestMethod]
    public void Role_ShouldCreateRole_WithValidData()
    {
        int id = 1;
        string name = "Administrator";

        Role role = new Role
        {
            Id = id,
            Name = name
        };

        Assert.AreEqual(id, role.Id);
        Assert.AreEqual(name, role.Name);
    }

    [TestMethod]
    public void Role_ShouldHaveAdministratorConstant()
    {
        Assert.AreEqual("Administrator", Role.ADMINISTRATOR);
    }

    [TestMethod]
    public void Role_ShouldHaveOperatorConstant()
    {
        Assert.AreEqual("Operator", Role.OPERATOR);
    }

    [TestMethod]
    public void Role_ShouldHaveVisitorConstant()
    {
        Assert.AreEqual("Visitor", Role.VISITOR);
    }
}
