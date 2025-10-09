using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.Out;

namespace TestBusinessLogic
{
    [TestClass]
    public class UserLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IPasswordLogic> _mockPasswordService;
        private Mock<IAttractionRepository> _mockAttractionRepository;
        private Mock<ITicketLogic> _mockTicketLogic;
        private Mock<IRoleRepository> _mockRoleRepository;
        private Mock<IEventRepository> _mockEventRepository;
        private Mock<IActiveStrategy> _mockActiveStrategy;
        private IUserLogic _userLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockPasswordService = new Mock<IPasswordLogic>();
            _mockAttractionRepository = new Mock<IAttractionRepository>();
            _mockTicketLogic = new Mock<ITicketLogic>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockEventRepository = new Mock<IEventRepository>();
            _mockActiveStrategy = new Mock<IActiveStrategy>();
            _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
                _mockAttractionRepository.Object, _mockTicketLogic.Object, _mockRoleRepository.Object,
                _mockEventRepository.Object, _mockActiveStrategy.Object);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldCreateVisitor_WithStandardMembership()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john.doe@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).ReturnsAsync(true);
            _mockPasswordService.Setup(p => p.HashPassword(password)).Returns(hashedPassword);

            User expectedUser = new User
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync(expectedUser);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(lastName, result.LastName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(hashedPassword, result.Password);
            Assert.AreEqual(birthDate, result.BirthDate);
            Assert.AreEqual(MembershipLevel.Standard, result.MembershipLevel);

            _mockUserRepository.Verify(r => r.IsEmailUnique(email), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword(password), Times.Once);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldReturnNull_WhenEmailIsNotUnique()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "existing@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).ReturnsAsync(false);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.IsEmailUnique(email), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldReturnNull_WhenEmailIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldReturnNull_WhenPasswordIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "";
            DateTime birthDate = new DateTime(1990, 5, 15);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldReturnNull_WhenNameIsEmpty()
        {
            string name = "";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldReturnNull_WhenLastNameIsEmpty()
        {
            string name = "John";
            string lastName = "";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, birthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldReturnNull_WhenBirthDateIsInFuture()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime futureBirthDate = DateTime.Now.AddDays(1);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, password, futureBirthDate);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RegisterVisitor_ShouldHashPassword_BeforeCreating()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string plainPassword = "plainPassword";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).ReturnsAsync(true);
            _mockPasswordService.Setup(p => p.HashPassword(plainPassword)).Returns(hashedPassword);

            User createdUser = new User
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.Is<User>(v => v.Password == hashedPassword)))
                .ReturnsAsync(createdUser);

            User result = await _userLogic.RegisterVisitor(name, lastName, email, plainPassword, birthDate);

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

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync((User)null);
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

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync((User)null);

            await _userLogic.RegisterExit(userId, attractionId, exitDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task RegisterExit_ShouldThrowExceptionWhenAttractionNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                VisitorReports = new List<VisitorReport>()
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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
            Guid eventId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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
            Guid eventId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            User visitor = new User
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
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
            Guid eventId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, eventId)).ReturnsAsync(false);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, eventId);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAddScoreToUser_WhenNoEvent()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Score = 0
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
                .ReturnsAsync((Event)null);
            _mockActiveStrategy.Setup(s => s.CalculateScore(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<StrategyRequest>()))
                .ReturnsAsync(5);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);

            Assert.AreEqual(5, visitor.Score);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                visitor,
                attraction,
                It.Is<StrategyRequest>(req => req.IsSepcialEvent == false)), Times.Once);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAddScoreToUser_WhenEventExists()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Score = 10
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Performance",
                Type = AttractionType.Performance,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            Event specialEvent = new Event
            {
                Id = Guid.NewGuid(),
                Name = "Special Event",
                Date = enterDate
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
                .ReturnsAsync(specialEvent);
            _mockActiveStrategy.Setup(s => s.CalculateScore(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<StrategyRequest>()))
                .ReturnsAsync(6);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);

            Assert.AreEqual(16, visitor.Score);
            _mockActiveStrategy.Verify(s => s.CalculateScore(
                visitor,
                attraction,
                It.Is<StrategyRequest>(req => req.IsSepcialEvent == true)), Times.Once);
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAccumulateScore_OverMultipleEntries()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId1 = Guid.NewGuid();
            Guid attractionId2 = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate1 = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime enterDate2 = new DateTime(2025, 10, 1, 11, 0, 0);

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Score = 0
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

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId1)).ReturnsAsync(attraction1);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId2)).ReturnsAsync(attraction2);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, It.IsAny<DateTime>(), null))
                .ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(It.IsAny<Guid>(), It.IsAny<DateTime>()))
                .ReturnsAsync((Event)null);
            _mockActiveStrategy.Setup(s => s.CalculateScore(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<StrategyRequest>()))
                .ReturnsAsync(3);

            await _userLogic.RegisterEntry(userId, attractionId1, enterDate1, qrCode, null, null);
            await _userLogic.RegisterEntry(userId, attractionId2, enterDate2, qrCode, null, null);

            Assert.AreEqual(6, visitor.Score);
            _mockActiveStrategy.Verify(s => s.CalculateScore(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<StrategyRequest>()), Times.Exactly(2));
        }

        [TestMethod]
        public async Task RegisterEntry_ShouldAddZeroScore_WhenStrategyReturnsZero()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Score = 5
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).ReturnsAsync(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicketAsync(qrCode, null, enterDate, null)).ReturnsAsync(true);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
                .ReturnsAsync((Event)null);
            _mockActiveStrategy.Setup(s => s.CalculateScore(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<StrategyRequest>()))
                .ReturnsAsync(0);

            await _userLogic.RegisterEntry(userId, attractionId, enterDate, qrCode, null, null);

            Assert.AreEqual(5, visitor.Score);
        }

        [TestMethod]
        public async Task GetTopTenUsers_ShouldReturnTopTenUsersOrderedByScore()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 100 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 90 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 80 },
                new User { Id = Guid.NewGuid(), Name = "User4", Score = 70 },
                new User { Id = Guid.NewGuid(), Name = "User5", Score = 60 },
                new User { Id = Guid.NewGuid(), Name = "User6", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User7", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User8", Score = 30 },
                new User { Id = Guid.NewGuid(), Name = "User9", Score = 20 },
                new User { Id = Guid.NewGuid(), Name = "User10", Score = 10 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).ReturnsAsync(expectedUsers);

            TopTenResponse result = await _userLogic.GetTopTenUsers();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(100, result.TopTenUsers[0].Score);
            Assert.AreEqual(10, result.TopTenUsers[9].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public async Task GetTopTenUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            List<User> emptyList = new List<User>();

            _mockUserRepository.Setup(r => r.GetTopTen()).ReturnsAsync(emptyList);

            TopTenResponse result = await _userLogic.GetTopTenUsers();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(0, result.TopTenUsers.Count);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public async Task GetTopTenUsers_ShouldReturnFewerThanTenUsers_WhenLessThanTenExist()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 30 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).ReturnsAsync(expectedUsers);

            TopTenResponse result = await _userLogic.GetTopTenUsers();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(3, result.TopTenUsers.Count);
            Assert.AreEqual(50, result.TopTenUsers[0].Score);
            Assert.AreEqual(30, result.TopTenUsers[2].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public async Task GetTopTenUsers_ShouldCallRepositoryGetTopTenOnce()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 100 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).ReturnsAsync(expectedUsers);

            await _userLogic.GetTopTenUsers();

            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public async Task GetTopTenUsers_ShouldReturnOnlyTenUsers_WhenMoreThanTenExist()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 110 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 100 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 90 },
                new User { Id = Guid.NewGuid(), Name = "User4", Score = 80 },
                new User { Id = Guid.NewGuid(), Name = "User5", Score = 70 },
                new User { Id = Guid.NewGuid(), Name = "User6", Score = 60 },
                new User { Id = Guid.NewGuid(), Name = "User7", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User8", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User9", Score = 30 },
                new User { Id = Guid.NewGuid(), Name = "User10", Score = 20 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).ReturnsAsync(expectedUsers);

            TopTenResponse result = await _userLogic.GetTopTenUsers();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.TopTenUsers);
            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(110, result.TopTenUsers[0].Score);
            Assert.AreEqual(20, result.TopTenUsers[9].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }
    }
}