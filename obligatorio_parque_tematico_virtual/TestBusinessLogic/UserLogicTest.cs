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
        private Mock<IAttractionRepository> _mockAttractionRepository;
        private IUserLogic _userLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockAttractionRepository = new Mock<IAttractionRepository>();
            _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object, _mockAttractionRepository.Object);
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

        [TestMethod]
        public async Task RegisterEntry_ShouldCreateNewVisitorReportWhenNoneExists()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(enterDate.Date, visitor.VisitorReports[0].Date.Date);
            _mockUserRepository.Verify(r => r.GetById(userId), Times.Once);
            _mockAttractionRepository.Verify(r => r.GetById(attractionId), Times.Once);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAddReportToExistingVisitorReport()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId1 = Guid.NewGuid();
            Guid attractionId2 = Guid.NewGuid();
            DateTime enterDate1 = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime enterDate2 = new DateTime(2025, 10, 1, 14, 0, 0);

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            Attraction attraction1 = new Attraction
            {
                Id = attractionId1,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };

            Attraction attraction2 = new Attraction
            {
                Id = attractionId2,
                Name = "Simulator",
                Type = AttractionType.Simulator
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId1)).ReturnsAsync(attraction1);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId2)).ReturnsAsync(attraction2);

            await _userLogic.RegisterEntry(userId, attractionId1, enterDate1);
            await _userLogic.RegisterEntry(userId, attractionId2, enterDate2);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(2, visitor.VisitorReports[0].Reports.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenAttractionNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync((Attraction)null);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate);
        }
    }
}