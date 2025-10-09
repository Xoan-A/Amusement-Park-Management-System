using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

namespace TestBusinessLogic;

[TestClass]
public class UserLogicRoleTest
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private Mock<IPasswordLogic> _mockPasswordService = null!;
    private Mock<IAttractionRepository> _mockAttractionRepository = null!;
    private Mock<ITicketLogic> _mockTicketLogic = null!;
    private Mock<IEventRepository> _mockEventRepository = null!;
    private Mock<IActiveStrategy> _mockActiveStrategy = null!;
    private IUserLogic _userLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockPasswordService = new Mock<IPasswordLogic>();
        _mockAttractionRepository = new Mock<IAttractionRepository>();
        _mockTicketLogic = new Mock<ITicketLogic>();
        _mockEventRepository = new Mock<IEventRepository>();
        _mockActiveStrategy = new Mock<IActiveStrategy>();
        _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
            _mockAttractionRepository.Object, _mockTicketLogic.Object, _mockRoleRepository.Object,
            _mockEventRepository.Object, _mockActiveStrategy.Object);
    }

    [TestMethod]
    public async Task RegisterVisitor_ShouldAssignVisitorRole()
    {
        string name = "John";
        string lastName = "Doe";
        string email = "john@test.com";
        string password = "password123";
        DateTime birthDate = new DateTime(1990, 1, 1);

        Role visitorRole = new Role { Id = 3, Name = Role.VISITOR };

        _mockUserRepository.Setup(r => r.IsEmailUnique(email)).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword(password)).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(visitorRole);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);

        RegisterVisitorRequest request = new RegisterVisitorRequest
        {
            Name = name,
            LastName = lastName,
            Email = email,
            Password = password,
            BirthDate = birthDate
        };

        User result = await _userLogic.RegisterVisitor(request);

        Assert.IsNotNull(result);
        _mockRoleRepository.Verify(r => r.GetByName(Role.VISITOR), Times.Once);
    }

    [TestMethod]
    public async Task CreateUser_ShouldCreateUserWithRoles()
    {
        string name = "Admin";
        string lastName = "User";
        string email = "admin@test.com";
        string password = "password123";
        string[] roles = new[] { Role.ADMINISTRATOR, Role.OPERATOR };

        Role adminRole = new Role { Id = 1, Name = Role.ADMINISTRATOR };
        Role operatorRole = new Role { Id = 2, Name = Role.OPERATOR };

        _mockUserRepository.Setup(r => r.IsEmailUnique(email)).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword(password)).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName(Role.ADMINISTRATOR)).Returns(adminRole);
        _mockRoleRepository.Setup(r => r.GetByName(Role.OPERATOR)).Returns(operatorRole);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = name,
            LastName = lastName,
            Email = email,
            Password = password,
            Roles = roles.ToList()
        };

        UserResponse result = await _userLogic.CreateUser(request);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.UserRoles.Count);
    }

    [TestMethod]
    public async Task CreateUser_ShouldReturnNull_WhenNameIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        UserResponse result = await _userLogic.CreateUser(request);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateUser_ShouldReturnNull_WhenLastNameIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        UserResponse result = await _userLogic.CreateUser(request);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateUser_ShouldReturnNull_WhenEmailIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        UserResponse result = await _userLogic.CreateUser(request);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateUser_ShouldReturnNull_WhenPasswordIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "",
            Roles = new List<string> { Role.VISITOR }
        };

        UserResponse result = await _userLogic.CreateUser(request);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateUser_ShouldReturnNull_WhenEmailIsNotUnique()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("existing@test.com")).ReturnsAsync(false);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "existing@test.com",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        UserResponse result = await _userLogic.CreateUser(request);
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateUser_ShouldHashPassword_BeforeCreating()
    {
        string plainPassword = "plainPassword123";
        string hashedPassword = "hashedPassword123";

        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword(plainPassword)).Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.Create(It.Is<User>(u => u.Password == hashedPassword)))
            .ReturnsAsync((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = plainPassword,
            Roles = null
        };

        UserResponse result = await _userLogic.CreateUser(request);

        Assert.IsNotNull(result);
        _mockPasswordService.Verify(p => p.HashPassword(plainPassword), Times.Once);
    }

    [TestMethod]
    public async Task CreateUser_ShouldCreateUserWithoutRoles_WhenRolesIsNull()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = null
        };

        UserResponse result = await _userLogic.CreateUser(request);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.UserRoles.Count);
    }

    [TestMethod]
    public async Task CreateUser_ShouldCreateUserWithoutRoles_WhenRolesIsEmpty()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string>()
        };

        UserResponse result = await _userLogic.CreateUser(request);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.UserRoles.Count);
    }

    [TestMethod]
    public async Task CreateUser_ShouldSkipInvalidRole_WhenRoleNotFound()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName("InvalidRole")).Returns((Role)null);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string>() { "InvalidRole" }
        };

        UserResponse result = await _userLogic.CreateUser(request);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.UserRoles.Count);
    }

    [TestMethod]
    public async Task CreateUser_ShouldCreateUserWithSingleRole()
    {
        Role visitorRole = new Role { Id = 3, Name = Role.VISITOR };

        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).ReturnsAsync(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(visitorRole);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string>() { Role.VISITOR }
        };

        UserResponse result = await _userLogic.CreateUser(request);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.UserRoles.Count);
        Assert.AreEqual(Role.VISITOR, result.UserRoles.First());
    }

    [TestMethod]
    public async Task AddRoleToUser_ShouldAddRoleSuccessfully()
    {
        Guid userId = Guid.NewGuid();
        string roleName = Role.OPERATOR;

        User user = new User
        {
            Id = userId,
            Name = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRoles = new List<UserRole>()
        };

        Role operatorRole = new Role { Id = 2, Name = Role.OPERATOR };

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(Task.FromResult(user));
        _mockRoleRepository.Setup(r => r.GetByName(roleName)).Returns(operatorRole);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        await _userLogic.AddRoleToUser(userId, roleName);

        Assert.AreEqual(1, user.UserRoles.Count);
        Assert.AreEqual(operatorRole, user.UserRoles.First().Role);
        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
    }

    [TestMethod]
    public async Task AddRoleToUser_ShouldThrowException_WhenUserNotFound()
    {
        Guid userId = Guid.NewGuid();
        string roleName = Role.OPERATOR;

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).ReturnsAsync((User)null);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            async () => await _userLogic.AddRoleToUser(userId, roleName),
            "User not found."
        );

        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task AddRoleToUser_ShouldThrowException_WhenRoleNotFound()
    {
        Guid userId = Guid.NewGuid();
        string roleName = "InvalidRole";

        User user = new User
        {
            Id = userId,
            Name = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRoles = new List<UserRole>()
        };

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(Task.FromResult(user));
        _mockRoleRepository.Setup(r => r.GetByName(roleName)).Returns((Role)null);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            async () => await _userLogic.AddRoleToUser(userId, roleName),
            "Role not found."
        );

        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task AddRoleToUser_ShouldThrowException_WhenUserAlreadyHasRole()
    {
        Guid userId = Guid.NewGuid();
        string roleName = Role.VISITOR;

        Role visitorRole = new Role { Id = 3, Name = Role.VISITOR };

        User user = new User
        {
            Id = userId,
            Name = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = visitorRole.Id, Role = visitorRole }
            }
        };

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(Task.FromResult(user));
        _mockRoleRepository.Setup(r => r.GetByName(roleName)).Returns(visitorRole);

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            async () => await _userLogic.AddRoleToUser(userId, roleName),
            "User already has that role."
        );

        Assert.AreEqual(1, user.UserRoles.Count);
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public async Task AddRoleToUser_ShouldAddSecondRole_WhenUserHasDifferentRole()
    {
        Guid userId = Guid.NewGuid();
        string newRoleName = Role.ADMINISTRATOR;

        Role visitorRole = new Role { Id = 3, Name = Role.VISITOR };
        Role adminRole = new Role { Id = 1, Name = Role.ADMINISTRATOR };

        User user = new User
        {
            Id = userId,
            Name = "John",
            LastName = "Doe",
            Email = "john@test.com",
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = visitorRole.Id, Role = visitorRole }
            }
        };

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(Task.FromResult(user));
        _mockRoleRepository.Setup(r => r.GetByName(newRoleName)).Returns(adminRole);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        await _userLogic.AddRoleToUser(userId, newRoleName);

        Assert.AreEqual(2, user.UserRoles.Count);
        Assert.IsTrue(user.UserRoles.Any(ur => ur.Role == visitorRole));
        Assert.IsTrue(user.UserRoles.Any(ur => ur.Role == adminRole));
        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
    }
}