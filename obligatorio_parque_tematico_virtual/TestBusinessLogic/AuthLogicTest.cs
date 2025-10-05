using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;

namespace TestBusinessLogic
{
    [TestClass]
    public class AuthLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IPasswordService> _mockPasswordService;
        private IAuthLogic _authLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _authLogic = new AuthLogic(_mockUserRepository.Object, _mockPasswordService.Object);
        }

        [TestMethod]
        public void Login_ShouldReturnUser_WhenCredentialsAreValid()
        {
            string email = "admin@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";

            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            User result = _authLogic.Login(email, password);

            Assert.IsNotNull(result);
            Assert.AreEqual(admin.Email, result.Email);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
        }

        [TestMethod]
        public void Login_ShouldReturnNull_WhenUserNotFound()
        {
            string email = "nonexistent@test.com";
            string password = "password123";

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns((User)null);

            User result = _authLogic.Login(email, password);

            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Login_ShouldReturnNull_WhenPasswordIsInvalid()
        {
            string email = "admin@test.com";
            string password = "wrongPassword";
            string hashedPassword = "hashedPassword123";

            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(false);

            User result = _authLogic.Login(email, password);

            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
        }

        [TestMethod]
        public void Login_ShouldWork_ForAllUserTypes()
        {
            string email = "visitor@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";

            Visitor visitor = new Visitor
            {
                Id = Guid.NewGuid(),
                Name = "Visitor",
                LastName = "User",
                Email = email,
                Password = hashedPassword,
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(visitor);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            User result = _authLogic.Login(email, password);

            Assert.IsNotNull(result);
            Assert.AreEqual(visitor.Email, result.Email);
        }

        [TestMethod]
        public void Login_ShouldHandleEmptyCredentials()
        {
            User result1 = _authLogic.Login("", "password");
            User result2 = _authLogic.Login("email@test.com", "");
            User result3 = _authLogic.Login(null, "password");
            User result4 = _authLogic.Login("email@test.com", null);

            Assert.IsNull(result1);
            Assert.IsNull(result2);
            Assert.IsNull(result3);
            Assert.IsNull(result4);
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

            _mockUserRepository.Setup(r => r.GetByEmailWithRoles(email)).Returns(user);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);

            User result = _authLogic.Login(email, password);

            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result.Email);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
        }
    }
}