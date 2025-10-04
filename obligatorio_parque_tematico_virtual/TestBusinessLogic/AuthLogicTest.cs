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
        private Mock<ITokenService> _mockTokenService;
        private IAuthLogic _authLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockTokenService = new Mock<ITokenService>();
            _authLogic = new AuthLogic(_mockUserRepository.Object, _mockPasswordService.Object, _mockTokenService.Object);
        }

        [TestMethod]
        public void Login_ShouldReturnToken_WhenCredentialsAreValid()
        {
            string email = "admin@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            string expectedToken = "jwt.token.here";

            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = email,
                Password = hashedPassword
            };

            _mockUserRepository.Setup(r => r.GetByEmail(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);
            _mockTokenService.Setup(t => t.GenerateToken(admin)).Returns(expectedToken);

            string result = _authLogic.Login(email, password);

            Assert.AreEqual(expectedToken, result);
            _mockUserRepository.Verify(r => r.GetByEmail(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
            _mockTokenService.Verify(t => t.GenerateToken(admin), Times.Once);
        }

        [TestMethod]
        public void Login_ShouldReturnNull_WhenUserNotFound()
        {
            string email = "nonexistent@test.com";
            string password = "password123";

            _mockUserRepository.Setup(r => r.GetByEmail(email)).Returns((User)null);

            string result = _authLogic.Login(email, password);

            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.GetByEmail(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
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

            _mockUserRepository.Setup(r => r.GetByEmail(email)).Returns(admin);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(false);

            string result = _authLogic.Login(email, password);

            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.GetByEmail(email), Times.Once);
            _mockPasswordService.Verify(p => p.VerifyPassword(password, hashedPassword), Times.Once);
            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public void Login_ShouldWork_ForAllUserTypes()
        {
            string email = "visitor@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            string expectedToken = "visitor.token.here";

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

            _mockUserRepository.Setup(r => r.GetByEmail(email)).Returns(visitor);
            _mockPasswordService.Setup(p => p.VerifyPassword(password, hashedPassword)).Returns(true);
            _mockTokenService.Setup(t => t.GenerateToken(visitor)).Returns(expectedToken);

            string result = _authLogic.Login(email, password);

            Assert.AreEqual(expectedToken, result);
        }

        [TestMethod]
        public void Login_ShouldHandleEmptyCredentials()
        {
            string result1 = _authLogic.Login("", "password");
            string result2 = _authLogic.Login("email@test.com", "");
            string result3 = _authLogic.Login(null, "password");
            string result4 = _authLogic.Login("email@test.com", null);

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
            string expectedToken = "jwt.token.here";

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
            _mockTokenService.Setup(t => t.GenerateToken(user)).Returns(expectedToken);

            string result = _authLogic.Login(email, password);

            Assert.AreEqual(expectedToken, result);
            _mockUserRepository.Verify(r => r.GetByEmailWithRoles(email), Times.Once);
        }
    }
}