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
    public class UserLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IPasswordService> _mockPasswordService;
        private IUserLogic _userLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldCreateVisitor_WithStandardMembership()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john.doe@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(password)).Returns(hashedPassword);

            Visitor expectedVisitor = new Visitor
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.IsAny<Visitor>())).Returns(expectedVisitor);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(lastName, result.LastName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(hashedPassword, result.Password);
            Assert.AreEqual(birthDate, result.BirthDate);
            Assert.AreEqual(MembershipLevel.Standard, result.MembershipLevel);

            _mockUserRepository.Verify(r => r.IsEmailUnique(email), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword(password), Times.Once);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<Visitor>()), Times.Once);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldReturnNull_WhenEmailIsNotUnique()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "existing@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(false);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.IsEmailUnique(email), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<Visitor>()), Times.Never);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldReturnNull_WhenEmailIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldReturnNull_WhenPasswordIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "";
            DateTime birthDate = new DateTime(1990, 5, 15);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldReturnNull_WhenNameIsEmpty()
        {
            string name = "";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldReturnNull_WhenLastNameIsEmpty()
        {
            string name = "John";
            string lastName = "";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldReturnNull_WhenBirthDateIsInFuture()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime futureBirthDate = DateTime.Now.AddDays(1);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, password, futureBirthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void RegisterVisitor_ShouldHashPassword_BeforeCreating()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string plainPassword = "plainPassword";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(plainPassword)).Returns(hashedPassword);

            Visitor createdVisitor = new Visitor
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.Is<Visitor>(v => v.Password == hashedPassword))).Returns(createdVisitor);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, plainPassword, birthDate);

            Assert.IsNotNull(result);
            Assert.AreEqual(hashedPassword, result.Password);
            _mockPasswordService.Verify(p => p.HashPassword(plainPassword), Times.Once);
        }
    }
}