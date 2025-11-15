using AutoMapper;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;
using Models.Mapping;

namespace TestBusinessLogic;

[TestClass]
public class UserLogicRoleTest
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IRoleRepository> _mockRoleRepository = null!;
    private Mock<IPasswordLogic> _mockPasswordService = null!;
    private Mock<IUserValidationService> _mockValidationService = null!;
    private Mock<IParkEntryLogic> _mockParkEntryLogic = null!;
    private IMapper _mapper = null!;
    private IUserLogic _userLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockPasswordService = new Mock<IPasswordLogic>();
        _mockValidationService = new Mock<IUserValidationService>();
        _mockParkEntryLogic = new Mock<IParkEntryLogic>();

        _mockValidationService.Setup(v => v.ValidateRequiredFields(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _mockValidationService.Setup(v => v.ValidateEmail(It.IsAny<string>())).Returns(true);
        _mockValidationService.Setup(v => v.ValidateBirthDate(It.IsAny<DateTime>()));
        _mockValidationService.Setup(v => v.ValidateEmailUniqueness(It.IsAny<string>()));
        _mockValidationService.Setup(v => v.ValidateMembershipLevel(It.IsAny<string>()));

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = configuration.CreateMapper();

        _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
            _mockRoleRepository.Object, _mockValidationService.Object, _mockParkEntryLogic.Object, _mapper);
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
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        RegisterVisitorRequest request = new RegisterVisitorRequest
        {
            Name = name,
            LastName = lastName,
            Email = email,
            Password = password,
            BirthDate = birthDate
        };

        UserResponse result = _userLogic.RegisterVisitor(request);

        Assert.AreEqual(email, result.Email);
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
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = name,
            LastName = lastName,
            Email = email,
            Password = password,
            Roles = roles.ToList()
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual(2, result.UserRoles.Count);
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenNameIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("", "Doe", "test@test.com", "password123"))
            .Throws(new ArgumentException("Name, last name, email, and password must be provided."));

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenLastNameIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("John", "", "test@test.com", "password123"))
            .Throws(new ArgumentException("Name, last name, email, and password must be provided."));

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenEmailIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("John", "Doe", "", "password123"))
            .Throws(new ArgumentException("Name, last name, email, and password must be provided."));

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenPasswordIsEmpty()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "",
            Roles = new List<string> { Role.VISITOR }
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("John", "Doe", "test@test.com", ""))
            .Throws(new ArgumentException("Name, last name, email, and password must be provided."));

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenEmailIsNotUnique()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "existing@test.com",
            Password = "password123",
            Roles = new List<string> { Role.VISITOR }
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("John", "Doe", "existing@test.com", "password123"));
        _mockValidationService.Setup(v => v.ValidateEmail("existing@test.com")).Returns(true);
        _mockValidationService.Setup(v => v.ValidateEmailUniqueness("existing@test.com"))
            .Throws(new ArgumentException("Email is already in use."));
        _mockUserRepository.Setup(r => r.IsEmailUnique("existing@test.com")).Returns(false);

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldHashPassword_BeforeCreating()
    {
        string plainPassword = "plainPassword123";
        string hashedPassword = "hashedPassword123";

        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword(plainPassword)).Returns(hashedPassword);
        _mockUserRepository.Setup(r => r.Create(It.Is<User>(u => u.Password == hashedPassword)))
        .Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = plainPassword,
            Roles = null
        };

        UserResponse result = _userLogic.CreateUser(request);

        _mockPasswordService.Verify(p => p.HashPassword(plainPassword), Times.Once);
    }

    [TestMethod]
    public void CreateUser_ShouldCreateUserWithoutRoles_WhenRolesIsNull()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = null
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual(0, result.UserRoles.Count);
    }

    [TestMethod]
    public void CreateUser_ShouldCreateUserWithoutRoles_WhenRolesIsEmpty()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string>()
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual(0, result.UserRoles.Count);
    }

    [TestMethod]
    public void CreateUser_ShouldSkipInvalidRole_WhenRoleNotFound()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName("InvalidRole")).Returns((Role)null);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string>() { "InvalidRole" }
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual(0, result.UserRoles.Count);
    }

    [TestMethod]
    public void CreateUser_ShouldCreateUserWithSingleRole()
    {
        Role visitorRole = new Role { Id = 3, Name = Role.VISITOR };

        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(visitorRole);
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            Roles = new List<string>() { Role.VISITOR }
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual(1, result.UserRoles.Count);
        Assert.AreEqual(Role.VISITOR, result.UserRoles.First());
    }

    [TestMethod]
    public void AddRoleToUser_ShouldAddRoleSuccessfully()
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

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
        _mockRoleRepository.Setup(r => r.GetByName(roleName)).Returns(operatorRole);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

        _userLogic.AddRoleToUser(userId, roleName);

        Assert.AreEqual(1, user.UserRoles.Count);
        Assert.AreEqual(operatorRole, user.UserRoles.First().Role);
        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
    }

    [TestMethod]
    public void AddRoleToUser_ShouldThrowException_WhenUserNotFound()
    {
        Guid userId = Guid.NewGuid();
        string roleName = Role.OPERATOR;

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.AddRoleToUser(userId, roleName),
            "User not found."
        );

        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public void AddRoleToUser_ShouldThrowException_WhenRoleNotFound()
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

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
        _mockRoleRepository.Setup(r => r.GetByName(roleName)).Returns((Role)null);

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.AddRoleToUser(userId, roleName),
            "Role not found."
        );

        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public void AddRoleToUser_ShouldThrowException_WhenUserAlreadyHasRole()
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

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
        _mockRoleRepository.Setup(r => r.GetByName(roleName)).Returns(visitorRole);

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.AddRoleToUser(userId, roleName),
            "User already has that role."
        );

        Assert.AreEqual(1, user.UserRoles.Count);
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [TestMethod]
    public void AddRoleToUser_ShouldAddSecondRole_WhenUserHasDifferentRole()
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

        _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
        _mockRoleRepository.Setup(r => r.GetByName(newRoleName)).Returns(adminRole);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

        _userLogic.AddRoleToUser(userId, newRoleName);

        Assert.AreEqual(2, user.UserRoles.Count);
        Assert.IsTrue(user.UserRoles.Any(ur => ur.Role == visitorRole));
        Assert.IsTrue(user.UserRoles.Any(ur => ur.Role == adminRole));
        _mockUserRepository.Verify(r => r.Update(user), Times.Once);
    }

    [TestMethod]
    public void CreateUser_ShouldSetBirthDate_WhenProvided()
    {
        DateTime birthDate = new DateTime(1990, 5, 15);
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            BirthDate = birthDate,
            Roles = new List<string>()
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual(birthDate, result.BirthDate);
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenBirthDateIsInFuture()
    {
        DateTime futureBirthDate = DateTime.Now.AddDays(1);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            BirthDate = futureBirthDate,
            Roles = new List<string>()
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("John", "Doe", "test@test.com", "password123"));
        _mockValidationService.Setup(v => v.ValidateEmail("test@test.com")).Returns(true);
        _mockValidationService.Setup(v => v.ValidateEmailUniqueness("test@test.com"));
        _mockValidationService.Setup(v => v.ValidateBirthDate(futureBirthDate))
            .Throws(new ArgumentException("Birth date cannot be after today."));
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldSetMembershipLevel_WhenProvided()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            MembershipLevel = "Premium",
            Roles = new List<string>()
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual((int)MembershipLevel.Premium, result.MembershipLevel);
    }

    [TestMethod]
    public void CreateUser_ShouldSetMembershipLevelVIP_WhenProvided()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");
        _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns((User u) => u);

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            MembershipLevel = "VIP",
            Roles = new List<string>()
        };

        UserResponse result = _userLogic.CreateUser(request);

        Assert.AreEqual((int)MembershipLevel.VIP, result.MembershipLevel);
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenMembershipLevelIsInvalid()
    {
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");

        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            MembershipLevel = "InvalidLevel",
            Roles = new List<string>()
        };

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }

    [TestMethod]
    public void CreateUser_ShouldThrowException_WhenMembershipLevelIsNumericButNotDefined()
    {
        CreateUserRequest request = new CreateUserRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "test@test.com",
            Password = "password123",
            MembershipLevel = "99",
            Roles = new List<string>()
        };

        _mockValidationService.Setup(v => v.ValidateRequiredFields("John", "Doe", "test@test.com", "password123"));
        _mockValidationService.Setup(v => v.ValidateEmail("test@test.com")).Returns(true);
        _mockValidationService.Setup(v => v.ValidateEmailUniqueness("test@test.com"));
        _mockValidationService.Setup(v => v.ValidateMembershipLevel("99"))
            .Throws(new ArgumentException("Invalid membership level."));
        _mockUserRepository.Setup(r => r.IsEmailUnique("test@test.com")).Returns(true);
        _mockPasswordService.Setup(p => p.HashPassword("password123")).Returns("hashedPassword");

        Assert.ThrowsException<ArgumentException>(
            () => _userLogic.CreateUser(request)
        );
    }
}