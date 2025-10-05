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
        private Mock<ITicketLogic> _mockTicketLogic;
        private IUserLogic _userLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockAttractionRepository = new Mock<IAttractionRepository>();
            _mockTicketLogic = new Mock<ITicketLogic>();
            _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
                _mockAttractionRepository.Object, _mockTicketLogic.Object);
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

            _mockUserRepository.Setup(r => r.Create(It.Is<Visitor>(v => v.Password == hashedPassword)))
                .Returns(createdVisitor);

            Visitor result = _userLogic.RegisterVisitor(name, lastName, email, plainPassword, birthDate);

            Assert.IsNotNull(result);
            Assert.AreEqual(hashedPassword, result.Password);
            _mockPasswordService.Verify(p => p.HashPassword(plainPassword), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenBothQrAndNfcAreNull()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenTicketValidationFails()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(false);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldCreateNewVisitorReportWhenNoneExists()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(enterDate.Date, visitor.VisitorReports[0].Date.Date);
            _mockUserRepository.Verify(r => r.GetById(userId), Times.Once);
            _mockAttractionRepository.Verify(r => r.GetById(attractionId), Times.Once);
            _mockTicketLogic.Verify(t => t.ValidateTicketAsync(qrCode, null, enterDate, null), Times.Once);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAddReportToExistingVisitorReport()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId1 = Guid.NewGuid();
            Guid attractionId2 = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            Attraction attraction2 = new Attraction
            {
                Id = attractionId2,
                Name = "Simulator",
                Type = AttractionType.Simulator,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId1)).ReturnsAsync(attraction1);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId2)).ReturnsAsync(attraction2);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate1, null)).ReturnsAsync(true);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate2, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId1, enterDate1, qrCode, null, null);
            await _userLogic.RegisterEntry(userId, attractionId2, enterDate2, qrCode, null, null);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(2, visitor.VisitorReports[0].Reports.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenAttractionNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
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
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldIncreaseCurrentCapacity()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 5
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);

            Assert.AreEqual(6, attraction.CurrentCapacity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenAttractionIsAtFullCapacity()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 10
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAllowEntryWhenCurrentCapacityIsJustBelowMax()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 9
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);

            Assert.AreEqual(10, attraction.CurrentCapacity);
            Assert.AreEqual(1, visitor.VisitorReports.Count);
        }

        [TestMethod]
        public async Task RegisterExit_ShouldSetExitTimeForReport()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
            await _userLogic.RegisterExit(userId, attractionId, exitDate);

            Assert.AreEqual(exitDate, visitor.VisitorReports[0].Reports[0].ExitDate);
            _mockUserRepository.Verify(r => r.GetById(userId), Times.Exactly(2));
            _mockAttractionRepository.Verify(r => r.GetById(attractionId), Times.Exactly(2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterExit_ShouldThrowExceptionWhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null);

            await _userLogic.RegisterExit(userId, attractionId, exitDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterExit_ShouldThrowExceptionWhenAttractionNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync((Attraction)null);

            await _userLogic.RegisterExit(userId, attractionId, exitDate);
        }

        [TestMethod]
        public async Task RegisterExit_ShouldDecreaseCurrentCapacity()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 5
            };

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
            await _userLogic.RegisterExit(userId, attractionId, exitDate);

            Assert.AreEqual(5, attraction.CurrentCapacity);
        }

        [TestMethod]
        public async Task RegisterExit_ShouldDecreaseCurrentCapacityToZero()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
            await _userLogic.RegisterExit(userId, attractionId, exitDate);

            Assert.AreEqual(0, attraction.CurrentCapacity);
        }

        [TestMethod]
        public async Task RegisterExit_ShouldDecreaseCurrentCapacityFromMaxCapacity()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 9
            };

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);
            await _userLogic.RegisterExit(userId, attractionId, exitDate);

            Assert.AreEqual(9, attraction.CurrentCapacity);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldWorkWithNfcInsteadOfQr()
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(null, userId, enterDate, null)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, null, userId, null);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(1, attraction.CurrentCapacity);
            _mockTicketLogic.Verify(t => t.ValidateTicketAsync(null, userId, enterDate, null), Times.Once);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldWorkWithEventId()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            int eventId = 5;
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
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, eventId)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, eventId);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(1, attraction.CurrentCapacity);
            _mockTicketLogic.Verify(t => t.ValidateTicketAsync(qrCode, null, enterDate, eventId), Times.Once);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldWorkWithBothQrAndEventId()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            int eventId = 10;
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            Visitor visitor = new Visitor
            {
                Id = userId,
                Name = "Jane",
                LastName = "Smith",
                VisitorReports = new List<VisitorReport>()
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Water Slide",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 20,
                CurrentCapacity = 5
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, eventId)).ReturnsAsync(true);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, eventId);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(6, attraction.CurrentCapacity);
            _mockTicketLogic.Verify(t => t.ValidateTicketAsync(qrCode, null, enterDate, eventId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterEntry_ShouldThrowExceptionWhenTicketInvalidForEvent()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            int eventId = 5;
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, eventId)).ReturnsAsync(false);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, eventId);
        }
    }
}