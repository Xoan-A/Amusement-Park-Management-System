using AutoMapper;
using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.Out;
using Domain.Exceptions;
using Models.In;
using Models.Mapping;

namespace TestBusinessLogic
{
    [TestClass]
    public class UserLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IPasswordLogic> _mockPasswordService = null!;
        private Mock<IAttractionRepository> _mockAttractionRepository = null!;
        private Mock<ITicketLogic> _mockTicketLogic = null!;
        private Mock<IRoleRepository> _mockRoleRepository = null!;
        private Mock<IEventRepository> _mockEventRepository = null!;
        private Mock<IDailyScoreLogic> _mockDailyScoreLogic = null!;
        private Mock<IDateTimeLogic> _mockDateTimeLogic = null!;
        private IMapper _mapper = null!;
        private IUserLogic _userLogic = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            _mockPasswordService = new Mock<IPasswordLogic>(MockBehavior.Strict);
            _mockAttractionRepository = new Mock<IAttractionRepository>(MockBehavior.Strict);
            _mockTicketLogic = new Mock<ITicketLogic>(MockBehavior.Strict);
            _mockRoleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
            _mockEventRepository = new Mock<IEventRepository>(MockBehavior.Strict);
            _mockDailyScoreLogic = new Mock<IDailyScoreLogic>(MockBehavior.Strict);
            _mockDateTimeLogic = new Mock<IDateTimeLogic>(MockBehavior.Strict);
            _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).Returns(DateTime.Now);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _userLogic = new UserLogic(_mockUserRepository.Object, _mockPasswordService.Object,
                _mockAttractionRepository.Object, _mockTicketLogic.Object, _mockRoleRepository.Object,
                _mockEventRepository.Object, _mockDailyScoreLogic.Object, _mockDateTimeLogic.Object, _mapper);
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
            _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(new Role { Name = Role.VISITOR });

            User expectedUser = new User
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>())).Returns(expectedUser);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            UserResponse result = _userLogic.RegisterVisitor(request);

            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(lastName, result.LastName);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(birthDate, result.BirthDate);

            _mockUserRepository.Verify(r => r.IsEmailUnique(email), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword(password), Times.Once);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailIsNotUnique()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "existing@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(false);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenPasswordIsEmpty()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "";
            DateTime birthDate = new DateTime(1990, 5, 15);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenNameIsEmpty()
        {
            string name = "";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenLastNameIsEmpty()
        {
            string name = "John";
            string lastName = "";
            string email = "john@test.com";
            string password = "password123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenBirthDateIsInFuture()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john@test.com";
            string password = "password123";
            DateTime futureBirthDate = DateTime.Now.AddDays(1);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = futureBirthDate
            };

            _userLogic.RegisterVisitor(request);
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
            _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns(new Role { Name = Role.VISITOR });

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
            .Returns(createdUser);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = plainPassword,
                BirthDate = birthDate
            };

            UserResponse result = _userLogic.RegisterVisitor(request);

            Assert.IsNotNull(result);
            _mockPasswordService.Verify(p => p.HashPassword(plainPassword), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterEntry_ShouldThrowExceptionWhenBothQrAndNfcAreNull()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = null,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterEntry_ShouldThrowExceptionWhenTicketValidationFails()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(false);

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);
        }

        [TestMethod]
        public void RegisterEntry_ShouldCreateNewVisitorReportWhenNoneExists()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(enterDate.Date, visitor.VisitorReports[0].Date.Date);
            _mockUserRepository.Verify(r => r.GetById(userId), Times.Once);
            _mockAttractionRepository.Verify(r => r.GetById(attractionId), Times.Once);
            _mockTicketLogic.Verify(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public void RegisterEntry_ShouldAddReportToExistingVisitorReport()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId1 = Guid.NewGuid();
            Guid attractionId2 = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId1)).Returns(attraction1);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId2)).Returns(attraction2);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(It.IsAny<Guid>(), enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, It.IsAny<Attraction>(), enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId1, request);
            _userLogic.RegisterEntry(attractionId2, request);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(2, visitor.VisitorReports[0].Reports.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterEntry_ShouldThrowExceptionWhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 0
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null);

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterEntry_ShouldThrowExceptionWhenAttractionNotFound()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns((Attraction)null);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);
        }

        [TestMethod]
        public void RegisterEntry_ShouldIncreaseCurrentCapacity()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);

            Assert.AreEqual(6, attraction.CurrentCapacity);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterEntry_ShouldThrowExceptionWhenAttractionIsAtFullCapacity()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);
        }

        [TestMethod]
        public void RegisterEntry_ShouldAllowEntryWhenCurrentCapacityIsJustBelowMax()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);

            Assert.AreEqual(10, attraction.CurrentCapacity);
            Assert.AreEqual(1, visitor.VisitorReports.Count);
        }

        [TestMethod]
        public void RegisterExit_ShouldSetExitTimeForReport()
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

            _mockDateTimeLogic.SetupSequence(d => d.GetCurrentDateTime())
            .Returns(enterDate)
            .Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, exitDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest entryRequest = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            RegisterExitRequest exitRequest = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterEntry(attractionId, entryRequest);
            _userLogic.RegisterExit(attractionId, exitRequest);

            Assert.AreEqual(exitDate, visitor.VisitorReports[0].Reports[0].ExitDate);
            _mockUserRepository.Verify(r => r.GetById(userId), Times.Exactly(2));
            _mockAttractionRepository.Verify(r => r.GetById(attractionId), Times.Exactly(2));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterExit_ShouldThrowExceptionWhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null);

            RegisterExitRequest request = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterExit(attractionId, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterExit_ShouldThrowExceptionWhenAttractionNotFound()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns((Attraction)null);

            RegisterExitRequest request = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterExit(attractionId, request);
        }

        [TestMethod]
        public void RegisterExit_ShouldDecreaseCurrentCapacity()
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

            _mockDateTimeLogic.SetupSequence(d => d.GetCurrentDateTime())
            .Returns(enterDate)
            .Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, exitDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest entryRequest = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            RegisterExitRequest exitRequest = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterEntry(attractionId, entryRequest);
            _userLogic.RegisterExit(attractionId, exitRequest);

            Assert.AreEqual(5, attraction.CurrentCapacity);
        }

        [TestMethod]
        public void RegisterExit_ShouldDecreaseCurrentCapacityToZero()
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

            _mockDateTimeLogic.SetupSequence(d => d.GetCurrentDateTime())
            .Returns(enterDate)
            .Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, exitDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest entryRequest = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            RegisterExitRequest exitRequest = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterEntry(attractionId, entryRequest);
            _userLogic.RegisterExit(attractionId, exitRequest);

            Assert.AreEqual(0, attraction.CurrentCapacity);
        }

        [TestMethod]
        public void RegisterExit_ShouldDecreaseCurrentCapacityFromMaxCapacity()
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

            _mockDateTimeLogic.SetupSequence(d => d.GetCurrentDateTime())
            .Returns(enterDate)
            .Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, exitDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest entryRequest = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            RegisterExitRequest exitRequest = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterEntry(attractionId, entryRequest);
            _userLogic.RegisterExit(attractionId, exitRequest);

            Assert.AreEqual(9, attraction.CurrentCapacity);
        }

        [TestMethod]
        public void RegisterEntry_ShouldWorkWithNfcInsteadOfQr()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(null, userId, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = null,
                NFC = userId,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(1, attraction.CurrentCapacity);
            _mockTicketLogic.Verify(t => t.ValidateTicket(null, userId, enterDate, null, It.IsAny<Guid>()), Times.Once);
        }

        [TestMethod]
        public void RegisterEntry_ShouldWorkWithEventId()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, eventId, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = eventId
            };

            _userLogic.RegisterEntry(attractionId, request);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(1, attraction.CurrentCapacity);
            _mockTicketLogic.Verify(t => t.ValidateTicket(qrCode, null, enterDate, eventId, It.IsAny<Guid>()),
                Times.Once);
        }

        [TestMethod]
        public void RegisterEntry_ShouldWorkWithBothQrAndEventId()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, eventId, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = eventId
            };

            _userLogic.RegisterEntry(attractionId, request);

            Assert.AreEqual(1, visitor.VisitorReports.Count);
            Assert.AreEqual(6, attraction.CurrentCapacity);
            _mockTicketLogic.Verify(t => t.ValidateTicket(qrCode, null, enterDate, eventId, It.IsAny<Guid>()),
                Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterEntry_ShouldThrowExceptionWhenTicketInvalidForEvent()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);

            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, eventId, It.IsAny<Guid>()))
            .Returns(false);

            RegisterEntryRequest request = new RegisterEntryRequest();

            _userLogic.RegisterEntry(attractionId, request);
        }

        [TestMethod]
        public void RegisterEntry_ShouldAddScoreToUser_WhenNoEvent()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);

            _mockDailyScoreLogic.Verify(s => s.AddScoreToUser(visitor, attraction, enterDate, null), Times.Once);
        }

        [TestMethod]
        public void RegisterEntry_ShouldAddScoreToUser_WhenEventExists()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, specialEvent.Id, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns(specialEvent);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, specialEvent));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = specialEvent.Id
            };

            _userLogic.RegisterEntry(attractionId, request);

            _mockDailyScoreLogic.Verify(s => s.AddScoreToUser(visitor, attraction, enterDate, specialEvent),
                Times.Once);
        }

        [TestMethod]
        public void RegisterEntry_ShouldAccumulateScore_OverMultipleEntries()
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

            _mockDateTimeLogic.SetupSequence(d => d.GetCurrentDateTime())
            .Returns(enterDate1)
            .Returns(enterDate2);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId1)).Returns(attraction1);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId2)).Returns(attraction2);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, It.IsAny<DateTime>(), null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d =>
            d.AddScoreToUser(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<DateTime>(), null));

            RegisterEntryRequest entryRequest = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            RegisterEntryRequest entryRequest2 = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId1, entryRequest);
            _userLogic.RegisterEntry(attractionId2, entryRequest2);

            _mockDailyScoreLogic.Verify(
                s => s.AddScoreToUser(It.IsAny<User>(), It.IsAny<Attraction>(), It.IsAny<DateTime>(), null),
                Times.Exactly(2));
        }

        [TestMethod]
        public void RegisterEntry_ShouldAddZeroScore_WhenStrategyReturnsZero()
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

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(enterDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _userLogic.RegisterEntry(attractionId, request);

            _mockDailyScoreLogic.Verify(s => s.AddScoreToUser(visitor, attraction, enterDate, null), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnTopTenUsersOrderedByScore()
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

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(100, result.TopTenUsers[0].Score);
            Assert.AreEqual(10, result.TopTenUsers[9].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            List<User> emptyList = new List<User>();

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(emptyList);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(0, result.TopTenUsers.Count);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnFewerThanTenUsers_WhenLessThanTenExist()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 50 },
                new User { Id = Guid.NewGuid(), Name = "User2", Score = 40 },
                new User { Id = Guid.NewGuid(), Name = "User3", Score = 30 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(3, result.TopTenUsers.Count);
            Assert.AreEqual(50, result.TopTenUsers[0].Score);
            Assert.AreEqual(30, result.TopTenUsers[2].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldCallRepositoryGetTopTenOnce()
        {
            List<User> expectedUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", Score = 100 }
            };

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            _userLogic.GetTopTenUsers();

            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void GetTopTenUsers_ShouldReturnOnlyTenUsers_WhenMoreThanTenExist()
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

            _mockUserRepository.Setup(r => r.GetTopTen()).Returns(expectedUsers);

            TopTenResponse result = _userLogic.GetTopTenUsers();

            Assert.AreEqual(10, result.TopTenUsers.Count);
            Assert.AreEqual(110, result.TopTenUsers[0].Score);
            Assert.AreEqual(20, result.TopTenUsers[9].Score);
            _mockUserRepository.Verify(r => r.GetTopTen(), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_ShouldUpdateAndReturnResponse_WhenDataIsValid()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "Old",
                LastName = "Name",
                Email = "old@example.com",
                Password = "oldpass",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard,
                UserRoles = new System.Collections.Generic.List<UserRole>
                {
                    new UserRole { Role = new Role { Name = Role.VISITOR } }
                },
                Score = 10
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "New",
                LastName = "Surname",
                Email = "new@example.com",
                Password = "New#Pass1",
                BirthDate = new DateTime(1992, 2, 2)
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.IsEmailUnique("new@example.com")).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual(userId, response.Id);
            Assert.AreEqual(request.Email, response.Email);

            _mockUserRepository.Verify(r => r.IsEmailUnique("new@example.com"), Times.Once);
            _mockPasswordService.Verify(p => p.HashPassword("New#Pass1"), Times.Once);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == request.Name &&
            u.LastName == request.LastName &&
            u.Email == request.Email &&
            u.Password == "hashed" &&
            u.BirthDate == request.BirthDate
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_WhenEmailNotChanged_DoesNotCheckUniqueness()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "Old",
                LastName = "Name",
                Email = "same@example.com",
                Password = "oldpass",
                BirthDate = new DateTime(1990, 1, 1)
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "New",
                LastName = "Surname",
                Email = "same@example.com",
                Password = "New#Pass1"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            _userLogic.ModifyUser(userId, actorSub, request);

            _mockUserRepository.Verify(r => r.IsEmailUnique(It.IsAny<string>()), Times.Never);
            _mockUserRepository.Verify(
                r => r.Update(It.Is<User>(u => u.Email == "same@example.com" && u.Password == "hashed")), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ForbiddenException))]
        public void ModifyUser_WhenActorSubIsGuidEmpty_ThrowsForbidden()
        {
            Guid userId = Guid.NewGuid();
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _userLogic.ModifyUser(userId, Guid.Empty, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ForbiddenException))]
        public void ModifyUser_WhenActorIsDifferentUser_ThrowsForbidden()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = Guid.NewGuid();
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ModifyUser_WhenUserNotFound_ThrowsNotFound()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "a@b.com",
                Password = "p"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ModifyUser_WhenEmailNotUnique_ThrowsArgument()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User { Id = userId, Name = "Old", LastName = "Name", Email = "old@example.com" };
            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "A",
                LastName = "B",
                Email = "new@example.com",
                Password = "p"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.IsEmailUnique("new@example.com")).Returns(false);

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        public void ModifyUser_OnlyUpdatesName_WhenOnlyNameProvided()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "NewName"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("NewName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("old@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "NewName" &&
            u.LastName == "OldLastName" &&
            u.Email == "old@example.com" &&
            u.Password == "oldHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_OnlyUpdatesEmail_WhenOnlyEmailProvided()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Email = "new@example.com"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.IsEmailUnique("new@example.com")).Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("OldName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("new@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "OldName" &&
            u.LastName == "OldLastName" &&
            u.Email == "new@example.com" &&
            u.Password == "oldHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_OnlyUpdatesPassword_WhenOnlyPasswordProvided()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Password = "newPassword123"
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockPasswordService.Setup(p => p.HashPassword("newPassword123")).Returns("newHashedPassword");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("OldName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("old@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "OldName" &&
            u.LastName == "OldLastName" &&
            u.Email == "old@example.com" &&
            u.Password == "newHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        public void ModifyUser_DoesNotUpdateAnything_WhenAllFieldsAreNull()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            User originalUser = new User
            {
                Id = userId,
                Name = "OldName",
                LastName = "OldLastName",
                Email = "old@example.com",
                Password = "oldHashedPassword",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest();

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse response = _userLogic.ModifyUser(userId, actorSub, request);

            Assert.AreEqual("OldName", response.Name);
            Assert.AreEqual("OldLastName", response.LastName);
            Assert.AreEqual("old@example.com", response.Email);
            _mockUserRepository.Verify(r => r.Update(It.Is<User>(u =>
            u.Name == "OldName" &&
            u.LastName == "OldLastName" &&
            u.Email == "old@example.com" &&
            u.Password == "oldHashedPassword"
            )), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ModifyUser_WhenBirthDateInFuture_ThrowsArgument()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSub = userId;
            DateTime currentDate = DateTime.Now;
            User originalUser = new User
            {
                Id = userId,
                Name = "Old",
                LastName = "Name",
                Email = "old@example.com",
                UserRoles = new List<UserRole>()
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                BirthDate = currentDate.AddDays(1)
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);

            _userLogic.ModifyUser(userId, actorSub, request);
        }

        [TestMethod]
        public void GetUserResponseById_ShouldReturnUserResponse_WhenUserExists()
        {
            Guid userId = Guid.NewGuid();
            User expectedUser = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                BirthDate = new DateTime(1990, 5, 15),
                MembershipLevel = MembershipLevel.Premium,
                Score = 100
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(expectedUser);

            UserResponse result = _userLogic.GetUserResponseById(userId);

            Assert.AreEqual(userId, result.Id);
            Assert.AreEqual("John", result.Name);
            Assert.AreEqual("john@test.com", result.Email);
            Assert.AreEqual(100, result.Score);

            _mockUserRepository.Verify(r => r.GetByIdWithRoles(userId), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetUserResponseById_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
            Guid userId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

            _userLogic.GetUserResponseById(userId);
        }

        [TestMethod]
        public void RegisterVisitor_WhenVisitorRoleNotFound_CreatesVisitorWithoutRole()
        {
            string name = "John";
            string lastName = "Doe";
            string email = "john.doe@test.com";
            string password = "password123";
            string hashedPassword = "hashedPassword123";
            DateTime birthDate = new DateTime(1990, 5, 15);

            _mockUserRepository.Setup(r => r.IsEmailUnique(email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(password)).Returns(hashedPassword);
            _mockRoleRepository.Setup(r => r.GetByName(Role.VISITOR)).Returns((Role)null);

            User createdUser = null;
            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns((User u) => u);

            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = password,
                BirthDate = birthDate
            };

            UserResponse result = _userLogic.RegisterVisitor(request);

            Assert.AreEqual(0, createdUser.UserRoles.Count, "User should have no roles when visitor role is not found");

            _mockRoleRepository.Verify(r => r.GetByName(Role.VISITOR), Times.Once);
            _mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public void CreateUser_WhenRolesIsNull_CreatesUserWithoutRoles()
        {
            CreateUserRequest request = new CreateUserRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                Roles = null
            };

            _mockUserRepository.Setup(r => r.IsEmailUnique(request.Email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");

            User createdUser = null;
            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns((User u) => u);

            _userLogic.CreateUser(request);

            Assert.AreEqual(0, createdUser.UserRoles.Count);
        }

        [TestMethod]
        public void CreateUser_WhenRoleNotFoundInDatabase_SkipsNonexistentRole()
        {
            CreateUserRequest request = new CreateUserRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                Roles = new List<string> { "Admin", "NonExistent" }
            };

            _mockUserRepository.Setup(r => r.IsEmailUnique(request.Email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("hashed");

            Role adminRole = new Role { Id = 1, Name = "Admin" };
            _mockRoleRepository.Setup(r => r.GetByName("Admin")).Returns(adminRole);
            _mockRoleRepository.Setup(r => r.GetByName("NonExistent")).Returns((Role)null);

            User createdUser = null;
            _mockUserRepository.Setup(r => r.Create(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns((User u) => u);

            _userLogic.CreateUser(request);

            Assert.AreEqual(1, createdUser.UserRoles.Count);
            Assert.AreEqual("Admin", createdUser.UserRoles.First().Role.Name);
        }

        [TestMethod]
        public void RegisterExit_WhenCapacityIsZero_DoesNotDecreaseCapacity()
        {
            Guid userId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Test Attraction",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 10,
                CurrentCapacity = 1
            };

            User visitor = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                VisitorReports = new List<VisitorReport>()
            };

            _mockDateTimeLogic.SetupSequence(d => d.GetCurrentDateTime())
            .Returns(enterDate)
            .Returns(exitDate);
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(visitor);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, enterDate, null, It.IsAny<Guid>()))
            .Returns(true);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, enterDate.Date))
            .Returns((Event?)null);
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, exitDate.Date))
            .Returns((Event?)null);
            _mockDailyScoreLogic.Setup(d => d.AddScoreToUser(visitor, attraction, enterDate, null));

            RegisterEntryRequest entryRequest = new RegisterEntryRequest
            {
                UserId = userId,
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            RegisterExitRequest exitRequest = new RegisterExitRequest
            {
                userId = userId
            };

            _userLogic.RegisterEntry(attractionId, entryRequest);
            attraction.CurrentCapacity = 0;
            _userLogic.RegisterExit(attractionId, exitRequest);

            Assert.AreEqual(0, attraction.CurrentCapacity, "Capacity should remain at zero and not go negative");
        }

        [TestMethod]
        public void ModifyUser_WhenBirthDateNotProvided_DoesNotUpdateBirthDate()
        {
            Guid userId = Guid.NewGuid();
            Guid actorSubClaim = userId;
            DateTime originalBirthDate = new DateTime(1990, 5, 15);

            User originalUser = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashed",
                BirthDate = originalBirthDate
            };

            ModifyUserRequest request = new ModifyUserRequest
            {
                Name = "Jane",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "newPassword123",
                BirthDate = null
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(originalUser);
            _mockUserRepository.Setup(r => r.IsEmailUnique(request.Email)).Returns(true);
            _mockPasswordService.Setup(p => p.HashPassword(request.Password)).Returns("newHashedPassword");
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            _userLogic.ModifyUser(userId, actorSubClaim, request);

            Assert.AreEqual(originalBirthDate, originalUser.BirthDate,
                "BirthDate should not change when null is provided");
            Assert.AreEqual("Jane", originalUser.Name);
        }


        [TestMethod]
        public void ChangeMembershipLevel_ValidLevel_UpdatesUserMembership()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 1;

            User user = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.Standard,
                Score = 100
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse result = _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            Assert.AreEqual(userId, result.Id);
            Assert.AreEqual(1, result.MembershipLevel);
            Assert.AreEqual(MembershipLevel.Premium, user.MembershipLevel);
            _mockUserRepository.Verify(r => r.GetByIdWithRoles(userId), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        public void ChangeMembershipLevel_ToVIP_UpdatesCorrectly()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 2;

            User user = new User
            {
                Id = userId,
                Name = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.Premium,
                Score = 500
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse result = _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            Assert.AreEqual(2, result.MembershipLevel);
            Assert.AreEqual(MembershipLevel.VIP, user.MembershipLevel);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        public void ChangeMembershipLevel_ToStandard_UpdatesCorrectly()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 0;

            User user = new User
            {
                Id = userId,
                Name = "Bob",
                LastName = "Johnson",
                Email = "bob@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.VIP,
                Score = 300
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            UserResponse result = _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            Assert.AreEqual(0, result.MembershipLevel);
            Assert.AreEqual(MembershipLevel.Standard, user.MembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ChangeMembershipLevel_InvalidLevel_ThrowsArgumentException()
        {
            Guid userId = Guid.NewGuid();
            int invalidMembershipLevel = 999;

            _userLogic.ChangeMembershipLevel(userId, invalidMembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ChangeMembershipLevel_NegativeLevel_ThrowsArgumentException()
        {
            Guid userId = Guid.NewGuid();
            int negativeMembershipLevel = -1;

            _userLogic.ChangeMembershipLevel(userId, negativeMembershipLevel);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ChangeMembershipLevel_UserNotFound_ThrowsKeyNotFoundException()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 1;

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns((User)null);

            _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);
        }

        [TestMethod]
        public void ChangeMembershipLevel_CallsRepositoryMethods_InCorrectOrder()
        {
            Guid userId = Guid.NewGuid();
            int newMembershipLevel = 1;

            User user = new User
            {
                Id = userId,
                Name = "Test",
                LastName = "User",
                Email = "test@test.com",
                Password = "hashed",
                MembershipLevel = MembershipLevel.Standard
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));

            _userLogic.ChangeMembershipLevel(userId, newMembershipLevel);

            _mockUserRepository.Verify(r => r.GetByIdWithRoles(userId), Times.Once);
            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailHasNoAtSymbol()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "invalidemail.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailStartsWithAt()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailEndsWithAt()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailHasMultipleAtSymbols()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailDomainHasNoDot()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@testcom",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailDomainStartsWithDot()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@.test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterVisitor_ShouldThrowException_WhenEmailDomainEndsWithDot()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "test@test.com.",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _userLogic.RegisterVisitor(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUser_ShouldThrowException_WhenEmailIsInvalid()
        {
            _mockUserRepository.Setup(r => r.IsEmailUnique(It.IsAny<string>())).Returns(true);

            CreateUserRequest request = new CreateUserRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "invalidemail",
                Password = "password123",
                Roles = new List<string>()
            };

            _userLogic.CreateUser(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ModifyUser_ShouldThrowException_WhenEmailIsInvalid()
        {
            Guid userId = Guid.NewGuid();
            User user = new User
            {
                Id = userId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashed",
                BirthDate = new DateTime(1990, 1, 1)
            };

            _mockUserRepository.Setup(r => r.GetByIdWithRoles(userId)).Returns(user);

            ModifyUserRequest request = new ModifyUserRequest
            {
                Email = "invalidemail"
            };

            _userLogic.ModifyUser(userId, userId, request);
        }
    }
}