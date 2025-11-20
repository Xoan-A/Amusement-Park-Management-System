using Moq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using BusinessLogic;
using Models.In;
using Models.Out;

namespace TestBusinessLogic
{
    [TestClass]
    public class ParkEntryLogicTest
    {
        private Mock<IUserRepository> _mockUserRepository = null!;
        private Mock<IAttractionRepository> _mockAttractionRepository = null!;
        private Mock<ITicketLogic> _mockTicketLogic = null!;
        private Mock<IEventRepository> _mockEventRepository = null!;
        private Mock<IDailyScoreLogic> _mockDailyScoreLogic = null!;
        private Mock<IDateTimeLogic> _mockDateTimeLogic = null!;
        private IParkEntryLogic _parkEntryLogic = null!;
        private DateTime _currentDateTime;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAttractionRepository = new Mock<IAttractionRepository>();
            _mockTicketLogic = new Mock<ITicketLogic>();
            _mockEventRepository = new Mock<IEventRepository>();
            _mockDailyScoreLogic = new Mock<IDailyScoreLogic>();
            _mockDateTimeLogic = new Mock<IDateTimeLogic>();

            _currentDateTime = new DateTime(2025, 1, 15, 10, 0, 0);
            _mockDateTimeLogic.Setup(x => x.GetCurrentDateTime()).Returns(_currentDateTime);

            _parkEntryLogic = new ParkEntryLogic(
                _mockUserRepository.Object,
                _mockAttractionRepository.Object,
                _mockTicketLogic.Object,
                _mockEventRepository.Object,
                _mockDailyScoreLogic.Object,
                _mockDateTimeLogic.Object
            );
        }

        [TestMethod]
        public void RegisterEntry_ShouldRegisterEntry_WhenQRCodeIsValid()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 10
            };

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, _currentDateTime, null, attractionId)).Returns(true);
            _mockTicketLogic.Setup(t => t.GetTicketByQRCode(qrCode)).Returns(new TicketResponse { VisitorId = userId });
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, _currentDateTime.Date)).Returns((Event)null!);

            _parkEntryLogic.RegisterEntry(attractionId, request);

            _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
            _mockAttractionRepository.Verify(r => r.Update(It.IsAny<Attraction>()), Times.Once);
            Assert.AreEqual(11, attraction.CurrentCapacity);
        }

        [TestMethod]
        public void RegisterEntry_ShouldThrowArgumentException_WhenQRAndNFCAreNull()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = null,
                NFC = null,
                EventId = null
            };

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterEntry(attractionId, request)
            );

            Assert.AreEqual("QR code or NFC must be provided.", exception.Message);
        }

        [TestMethod]
        public void RegisterEntry_ShouldThrowArgumentException_WhenAttractionNotFound()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns((Attraction)null!);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterEntry(attractionId, request)
            );

            Assert.AreEqual("Attraction not found.", exception.Message);
        }

        [TestMethod]
        public void RegisterEntry_ShouldThrowArgumentException_WhenTicketIsInvalid()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 10
            };

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
                NFC = null,
                EventId = null
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, _currentDateTime, null, attractionId)).Returns(false);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterEntry(attractionId, request)
            );

            Assert.AreEqual("User does not have a valid ticket.", exception.Message);
        }

        [TestMethod]
        public void RegisterEntry_ShouldThrowArgumentException_WhenUserNotFound()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 10
            };

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, _currentDateTime, null, attractionId)).Returns(true);
            _mockTicketLogic.Setup(t => t.GetTicketByQRCode(qrCode)).Returns(new TicketResponse { VisitorId = userId });
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null!);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterEntry(attractionId, request)
            );

            Assert.AreEqual("User not found.", exception.Message);
        }

        [TestMethod]
        public void RegisterEntry_ShouldThrowArgumentException_WhenAttractionIsAtFullCapacity()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 50
            };

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, _currentDateTime, null, attractionId)).Returns(true);
            _mockTicketLogic.Setup(t => t.GetTicketByQRCode(qrCode)).Returns(new TicketResponse { VisitorId = userId });
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterEntry(attractionId, request)
            );

            Assert.AreEqual("Attraction is at full capacity.", exception.Message);
        }

        [TestMethod]
        public void RegisterExit_ShouldRegisterExit_WhenUserAndAttractionExist()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 10
            };

            user.RegisterEntry(attraction, _currentDateTime);

            RegisterExitRequest request = new RegisterExitRequest { userId = userId };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));

            _parkEntryLogic.RegisterExit(attractionId, request);

            _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
            _mockAttractionRepository.Verify(r => r.Update(It.IsAny<Attraction>()), Times.Once);
            Assert.AreEqual(9, attraction.CurrentCapacity);
        }

        [TestMethod]
        public void RegisterExit_ShouldThrowArgumentException_WhenUserNotFound()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            RegisterExitRequest request = new RegisterExitRequest { userId = userId };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns((User)null!);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterExit(attractionId, request)
            );

            Assert.AreEqual("User not found.", exception.Message);
        }

        [TestMethod]
        public void RegisterExit_ShouldThrowArgumentException_WhenAttractionNotFound()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            RegisterExitRequest request = new RegisterExitRequest { userId = userId };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns((Attraction)null!);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterExit(attractionId, request)
            );

            Assert.AreEqual("Attraction not found.", exception.Message);
        }

        [TestMethod]
        public void RegisterExit_ShouldNotDecrementCapacity_WhenCurrentCapacityIsZero()
        {
            Guid attractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 0
            };

            user.RegisterEntry(attraction, _currentDateTime);

            RegisterExitRequest request = new RegisterExitRequest { userId = userId };

            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);
            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));

            _parkEntryLogic.RegisterExit(attractionId, request);

            Assert.AreEqual(0, attraction.CurrentCapacity);
            _mockAttractionRepository.Verify(r => r.Update(It.IsAny<Attraction>()), Times.Never);
        }

        [TestMethod]
        public void RegisterEntry_ShouldThrowArgumentException_WhenUserIsInsideAttractionWithoutExit()
        {
            Guid attractionId = Guid.NewGuid();
            Guid anotherAttractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 10
            };

            Attraction anotherAttraction = new Attraction
            {
                Id = anotherAttractionId,
                Name = "Ferris Wheel",
                Description = "Slow ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 100,
                CurrentCapacity = 20
            };

            user.RegisterEntry(anotherAttraction, _currentDateTime);

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, _currentDateTime, null, attractionId)).Returns(true);
            _mockTicketLogic.Setup(t => t.GetTicketByQRCode(qrCode)).Returns(new TicketResponse { VisitorId = userId });
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _parkEntryLogic.RegisterEntry(attractionId, request)
            );

            Assert.AreEqual("User is currently inside an attraction.", exception.Message);
        }

        [TestMethod]
        public void RegisterEntry_ShouldSucceed_WhenUserHasNoUnfinishedReports()
        {
            Guid attractionId = Guid.NewGuid();
            Guid anotherAttractionId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();

            User user = new User { Id = userId, Name = "John", LastName = "Doe", Email = "john@example.com" };
            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Roller Coaster",
                Description = "Fast ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 50,
                CurrentCapacity = 10
            };

            Attraction anotherAttraction = new Attraction
            {
                Id = anotherAttractionId,
                Name = "Ferris Wheel",
                Description = "Slow ride",
                Type = AttractionType.RollerCoaster,
                MaxCapacity = 100,
                CurrentCapacity = 20
            };

            DateTime enterTime = _currentDateTime.AddHours(-1);
            DateTime exitTime = _currentDateTime.AddMinutes(-30);
            user.RegisterEntry(anotherAttraction, enterTime);
            user.RegisterExit(anotherAttraction, exitTime);

            RegisterEntryRequest request = new RegisterEntryRequest
            {
                Qr = qrCode,
            };

            _mockAttractionRepository.Setup(r => r.GetById(attractionId)).Returns(attraction);
            _mockTicketLogic.Setup(t => t.ValidateTicket(qrCode, null, _currentDateTime, null, attractionId)).Returns(true);
            _mockTicketLogic.Setup(t => t.GetTicketByQRCode(qrCode)).Returns(new TicketResponse { VisitorId = userId });
            _mockUserRepository.Setup(r => r.GetById(userId)).Returns(user);
            _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
            _mockAttractionRepository.Setup(r => r.Update(It.IsAny<Attraction>()));
            _mockEventRepository.Setup(r => r.GetEventByAttractionAndDate(attractionId, _currentDateTime.Date)).Returns((Event)null!);

            _parkEntryLogic.RegisterEntry(attractionId, request);

            _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Once);
            _mockAttractionRepository.Verify(r => r.Update(It.IsAny<Attraction>()), Times.Once);
            Assert.AreEqual(11, attraction.CurrentCapacity);
        }
    }
}
