using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;

namespace TestBusinessLogic;

[TestClass]
public class UserLogicRoleTest
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private Mock<IPasswordService> _mockPasswordService = null!;
    private Mock<IAttractionRepository> _mockAttractionRepository = null!;
    private Mock<ITicketLogic> _mockTicketLogic = null!;
    private IUserLogic _userLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockTicketLogic = new Mock<ITicketLogic>();
        _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
            _mockAttractionRepository.Object, _mockTicketLogic.Object, _mockRoleRepository.Object);
    }

    [TestMethod]
    public void RegisterVisitor_ShouldAssignVisitorRole()
    {
        string name = "John";
        string lastName = "Doe";
        string email = "john@test.com";
        string password = "password123";
        DateTime birthDate = new DateTime(1990, 1, 1);

        Role visitorRole = new Role { Id = 3, Name = Role.VISITOR };

        _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword(password)).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(visitorRole);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns<User>(u => u);

        User result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

        Assert.IsNotNull(result);
        _mockRoleRepository.Verify(r => r.GetByName(Role.VISITOR), Times.Once);
    }

    [TestMethod]
    public void CreateUser_ShouldCreateUserWithRoles()
    {
        string name = "Admin";
        string lastName = "User";
        string email = "admin@test.com";
        string password = "password123";
        string[] roles = new[] { Role.ADMINISTRATOR, Role.OPERATOR };

        Role adminRole = new Role { Id = 1, Name = Role.ADMINISTRATOR };
        Role operatorRole = new Role { Id = 2, Name = Role.OPERATOR };

        _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword(password)).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName(Role.ADMINISTRATOR)).Returns(adminRole);
        _mockRoleRepository.Setup(r => r.GetByName(Role.OPERATOR)).Returns(operatorRole);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns<User>(u => u);

        User result = _userLogic.CreateUser(name, lastName, email, password, roles);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.UserRoles.Count);
    }
}
