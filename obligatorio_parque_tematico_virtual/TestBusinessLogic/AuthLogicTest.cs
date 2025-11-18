using AutoMapper;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.Out;
using BusinessLogic.Mapping;

namespace TestBusinessLogic
{
    [TestClass]
    public class AuthLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IPasswordLogic> _mockPasswordService = null!;
        private IMapper _mapper = null!;
        private IAuthLogic _authLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            _mockPasswordService = new Mock<IPasswordLogic>(MockBehavior.Strict);

            MapperConfiguration configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _authLogic = new AuthLogic(_mockUserRepository.Object, _mockPasswordService.Object, _mapper);
        }

        [TestMethod]
        public void Login_ShouldReturnUserResponse_WhenCredentialsAreValid()
        {
            string email = "admin@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";

            User admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.Administrator } }
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            UserResponse result = _authLogic.Login(email, password);

            Assert.IsNotNull(result);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
        }

        [TestMethod]
        public void Login_ShouldReturnCorrectEmail_WhenCredentialsAreValid()
        {
            string email = "admin@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";

            User admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.Administrator } }
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            UserResponse result = _authLogic.Login(email, password);

            Assert.AreEqual(admin.Email, result.Email);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
        }

        [TestMethod]
        public void Login_ShouldThrowArgumentException_WhenUserNotFound()
        {
            string email = "nonexistent@test.com";
            string password = "password123";

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns((User)null!);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _authLogic.Login(email, password)
            );

            Assert.AreEqual("Invalid email or password.", exception.Message);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Login_ShouldThrowArgumentException_WhenPasswordIsInvalid()
        {
            string email = "admin@test.com";
            string password = "wrongPassword";
            string hashedPassword = "hashedPassword123";

            User admin = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };
            admin.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.Administrator } }
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(false);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _authLogic.Login(email, password)
            );

            Assert.AreEqual("Invalid email or password.", exception.Message);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
        }

        [TestMethod]
        public void Login_ShouldWork_ForAllUserTypes()
        {
            string email = "visitor@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";

            User visitor = new User
            {
                Id = Guid.NewGuid(),
                Name = "Visitor",
                LastName = "User",
                Email = email,
                Password = hashedPassword,
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard
            };
            visitor.UserRoles = new System.Collections.Generic.List<UserRole>();

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(visitor);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            UserResponse result = _authLogic.Login(email, password);

            Assert.AreEqual(visitor.Email, result.Email);
        }

        [TestMethod]
        public void Login_ShouldThrowArgumentException_ForEmptyEmail()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _authLogic.Login("", "password")
            );

            Assert.AreEqual("Email and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void Login_ShouldThrowArgumentException_ForEmptyPassword()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _authLogic.Login("email@test.com", "")
            );

            Assert.AreEqual("Email and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void Login_ShouldThrowArgumentException_ForNullEmail()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _authLogic.Login(null!, "password")
            );

            Assert.AreEqual("Email and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void Login_ShouldThrowArgumentException_ForNullPassword()
        {
            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _authLogic.Login("email@test.com", null!)
            );

            Assert.AreEqual("Email and password must be provided.", exception.Message);
        }

        [TestMethod]
        public void Login_ShouldLoadUserWithRoles()
        {
            string email = "admin@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";

            User user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };
            user.UserRoles = new System.Collections.Generic.List<UserRole>
            {
                new UserRole { Role = new Role { Name = Role.Administrator } },
                new UserRole { Role = new Role { Name = Role.Operator } }
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(user);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            UserResponse result = _authLogic.Login(email, password);

            Assert.AreEqual(user.Email, result.Email);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
        }
    }
}