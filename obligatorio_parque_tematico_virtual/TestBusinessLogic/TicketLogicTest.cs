using BusinessLogic;
using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;
using Moq;

namespace TestBusinessLogic
{
    [TestClass]
    public class TicketLogicTest
    {
        private Mock<ITicketRepository> _mockTicketRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IDateTimeLogic> _mockDateTimeLogic;
        private Mock<IEventRepository> _mockEventRepository;
        private ITicketLogic _ticketLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockTicketRepository = new Mock<ITicketRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockDateTimeLogic = new Mock<IDateTimeLogic>();
            _mockEventRepository = new Mock<IEventRepository>();
            _ticketLogic = new TicketLogic(_mockTicketRepository.Object, _mockUserRepository.Object,
                _mockDateTimeLogic.Object, _mockEventRepository.Object);
        }

        [TestMethod]
        public void TestPurchaseTicket_Success()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 15, 0, 0, 0);

            User visitor = new User
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword"
            };

            Guid ticketId = Guid.NewGuid();
            Ticket expectedTicket = new Ticket
            {
                Id = ticketId,
                VisitorId = visitorId,
                PurchaseDate = currentDate,
                VisitDate = visitDate,
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);
            _mockTicketRepository.Setup(t => t.Add(It.IsAny<Ticket>())).Returns(expectedTicket);
            _mockTicketRepository.Setup(t => t.GetById(expectedTicket.Id)).Returns(new Ticket
            {
                Id = expectedTicket.Id,
                VisitorId = expectedTicket.VisitorId,
                PurchaseDate = expectedTicket.PurchaseDate,
                VisitDate = expectedTicket.VisitDate,
                Type = expectedTicket.Type,
                QRCode = expectedTicket.QRCode,
                Visitor = visitor
            });

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = (int)TicketType.General
            };

            TicketResponse result = _ticketLogic.PurchaseTicket(request);

            Assert.AreEqual(visitorId, result.VisitorId);
            Assert.AreEqual((int)TicketType.General, result.Type);
            Assert.AreNotEqual(Guid.Empty, result.QRCode);
            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Once);
        }

        [TestMethod]
        public void TestPurchaseTicket_VisitorNotFound()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime visitDate = DateTime.Now.AddDays(7);

            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns((User)null);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = (int)TicketType.General
            };

            Assert.ThrowsException<KeyNotFoundException>(
                () => _ticketLogic.PurchaseTicket(request)
            );

            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public void TestPurchaseTicket_PastVisitDate()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime pastVisitDate = new DateTime(2025, 1, 10, 0, 0, 0);

            User visitor = new User
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com"
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = pastVisitDate,
                TicketType = (int)TicketType.General
            };

            Assert.ThrowsException<ArgumentException>(
                () => _ticketLogic.PurchaseTicket(request)
            );

            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public void TestPurchaseTicket_InvalidTicketType()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 15, 0, 0, 0);

            User visitor = new User
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com"
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = 999
            };

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _ticketLogic.PurchaseTicket(request)
            );

            Assert.IsTrue(exception.Message.Contains("Invalid ticket type"));
            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public void TestPurchaseTicket_EventSpecialWithoutEventId_ThrowsException()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 15, 0, 0, 0);

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = (int)TicketType.EventSpecial,
                EventId = null
            };

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _ticketLogic.PurchaseTicket(request)
            );

            Assert.IsTrue(exception.Message.Contains("EventSpecial ticket type requires an event ID"));
            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public void TestPurchaseTicket_EventSpecialWithNonExistentEvent_ThrowsException()
        {
            Guid visitorId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 15, 0, 0, 0);

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns((Event)null);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = (int)TicketType.EventSpecial,
                EventId = eventId
            };

            KeyNotFoundException exception = Assert.ThrowsException<KeyNotFoundException>(
                () => _ticketLogic.PurchaseTicket(request)
            );

            Assert.IsTrue(exception.Message.Contains($"Event with ID {eventId} not found"));
            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public void TestPurchaseTicket_EventSpecialWithMismatchingDate_ThrowsException()
        {
            Guid visitorId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime eventDate = new DateTime(2025, 1, 15, 14, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 20, 0, 0, 0);

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Special Event",
                Date = eventDate,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = (int)TicketType.EventSpecial,
                EventId = eventId
            };

            ArgumentException exception = Assert.ThrowsException<ArgumentException>(
                () => _ticketLogic.PurchaseTicket(request)
            );

            Assert.IsTrue(exception.Message.Contains("Visit date must match the event date"));
            _mockTicketRepository.Verify(t => t.Add(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public void TestPurchaseTicket_EventSpecialType()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 20, 14, 0, 0);
            Guid eventId = Guid.NewGuid();

            User visitor = new User
            {
                Id = visitorId,
                Name = "Jane",
                LastName = "Smith"
            };

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Special Event",
                Date = visitDate,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50
            };

            Guid ticketId = Guid.NewGuid();
            Ticket expectedTicket = new Ticket
            {
                Id = ticketId,
                VisitorId = visitorId,
                PurchaseDate = currentDate,
                VisitDate = visitDate,
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = eventId
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);
            _mockTicketRepository.Setup(t => t.Add(It.IsAny<Ticket>())).Returns(expectedTicket);
            _mockTicketRepository.Setup(t => t.GetById(expectedTicket.Id)).Returns(new Ticket
            {
                Id = expectedTicket.Id,
                VisitorId = expectedTicket.VisitorId,
                PurchaseDate = expectedTicket.PurchaseDate,
                VisitDate = expectedTicket.VisitDate,
                Type = expectedTicket.Type,
                QRCode = expectedTicket.QRCode,
                EventId = expectedTicket.EventId,
                Visitor = visitor
            });

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = visitorId,
                VisitDate = visitDate,
                TicketType = (int)TicketType.EventSpecial,
                EventId = eventId
            };

            TicketResponse result = _ticketLogic.PurchaseTicket(request);

            Assert.AreEqual((int)TicketType.EventSpecial, result.Type);
            Assert.AreEqual(eventId, result.EventId);
        }

        [TestMethod]
        public void TestGetTicketById()
        {
            Guid ticketId = Guid.NewGuid();
            Ticket expectedTicket = new Ticket
            {
                Id = ticketId,
                VisitorId = Guid.NewGuid(),
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetById(ticketId)).Returns(expectedTicket);

            TicketResponse result = _ticketLogic.GetTicketById(ticketId);

            Assert.AreEqual(ticketId, result.Id);
            _mockTicketRepository.Verify(t => t.GetById(ticketId), Times.Once);
        }

        [TestMethod]
        public void TestGetVisitorTickets()
        {
            Guid visitorId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();

            User visitor = new User
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com"
            };

            List<Ticket> expectedTickets = new List<Ticket>
            {
                new Ticket { Id = Guid.NewGuid(), VisitorId = visitorId, Type = TicketType.General },
                new Ticket
                {
                    Id = Guid.NewGuid(), VisitorId = visitorId, Type = TicketType.EventSpecial, EventId = eventId
                }
            };

            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);
            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(expectedTickets);

            IEnumerable<TicketResponse> result = _ticketLogic.GetVisitorTickets(visitorId);

            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(t => t.VisitorId == visitorId));
            _mockUserRepository.Verify(u => u.GetById(visitorId), Times.Once);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestGetVisitorTickets_VisitorNotFound()
        {
            Guid visitorId = Guid.NewGuid();

            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns((User)null);

            Assert.ThrowsException<KeyNotFoundException>(
                () => _ticketLogic.GetVisitorTickets(visitorId)
            );

            _mockUserRepository.Verify(u => u.GetById(visitorId), Times.Once);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void TestGetTicketByQRCode()
        {
            Guid qrCode = Guid.NewGuid();
            Ticket expectedTicket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(expectedTicket);

            TicketResponse result = _ticketLogic.GetTicketByQRCode(qrCode);

            Assert.AreEqual(qrCode, result.QRCode);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_BothQrAndNfcNull_ReturnsFalse()
        {
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;
            Guid attractionId = Guid.NewGuid();

            bool result = _ticketLogic.ValidateTicket(null, null, enterDate, eventId, attractionId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(It.IsAny<Guid>()), Times.Never);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public void TestValidateTicket_ValidQrWithMatchingDateAndEventId_ReturnsTrue()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                EventId = eventId,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidQrWithMatchingDateButMismatchingEventId_ReturnsFalse()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid eventId = Guid.NewGuid();
            Guid differentEventId = Guid.NewGuid();

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                EventId = differentEventId,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidQrWithMismatchingDate_ReturnsFalse()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 20, 0, 0, 0),
                EventId = eventId,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_QrCodeNotFound_ReturnsFalse()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns((Ticket)null);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidNfcWithMatchingDateAndEventId_ReturnsTrue()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = eventId,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(tickets);

            bool result = _ticketLogic.ValidateTicket(null, visitorId, enterDate, eventId, Guid.NewGuid());

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidNfcWithMatchingDateButMismatchingEventId_ReturnsFalse()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid eventId = Guid.NewGuid();
            Guid differentEventId = Guid.NewGuid();

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = differentEventId,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(tickets);

            bool result = _ticketLogic.ValidateTicket(null, visitorId, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidNfcWithMismatchingDate_ReturnsFalse()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 20, 0, 0, 0),
                    EventId = eventId,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(tickets);

            bool result = _ticketLogic.ValidateTicket(null, visitorId, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_NfcWithNoTickets_ReturnsFalse()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;

            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(new List<Ticket>());

            bool result = _ticketLogic.ValidateTicket(null, visitorId, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidQrWithNullEventId_ReturnsTrue()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                EventId = null,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, null, Guid.NewGuid());

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_ValidNfcWithNullEventId_ReturnsTrue()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = null,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(tickets);

            bool result = _ticketLogic.ValidateTicket(null, visitorId, enterDate, null, Guid.NewGuid());

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_NfcWithMultipleTicketsFindsCorrectOne_ReturnsTrue()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            Guid? eventId = null;
            Guid wrongEventId1 = Guid.NewGuid();
            Guid wrongEventId2 = Guid.NewGuid();

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 10, 0, 0, 0),
                    EventId = wrongEventId1,
                    Type = TicketType.General
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = eventId,
                    Type = TicketType.General
                },
                new Ticket
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 20, 0, 0, 0),
                    EventId = wrongEventId2,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorId(visitorId)).Returns(tickets);

            bool result = _ticketLogic.ValidateTicket(null, visitorId, enterDate, eventId, Guid.NewGuid());

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorId(visitorId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_EventTimeWithinWindow_ReturnsTrue()
        {
            Guid eventId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime eventDateStart = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime enterDate = new DateTime(2025, 1, 15, 11, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = eventDateStart.Date,
                EventId = eventId,
                Type = TicketType.EventSpecial
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Main Stage",
            };

            EventAttraction eventAttraction = new EventAttraction
            {
                EventId = eventId,
                AttractionId = attraction.Id,
                Attraction = attraction
            };

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Concierto",
                Date = eventDateStart,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50,
                Attractions = new List<EventAttraction> { eventAttraction }
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, attractionId);

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
            _mockEventRepository.Verify(e => e.GetById(eventId), Times.Once);
        }

        [TestMethod]
        public void TestValidateTicket_EventTimeOutsideWindow_ReturnsFalse()
        {
            Guid eventId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime eventDateStart = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime enterDate = new DateTime(2025, 1, 15, 15, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = eventDateStart.Date,
                EventId = eventId,
                Type = TicketType.EventSpecial
            };

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Concierto",
                Date = eventDateStart,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
            _mockEventRepository.Verify(e => e.GetById(eventId), Times.Once);
        }

        [TestMethod]
        public void ValidateTicket_WhenTicketHasNoEvent_SkipsEventValidation()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime visitDate = new DateTime(2025, 1, 15);
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = visitDate,
                EventId = null,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, null, Guid.NewGuid());

            Assert.IsTrue(result, "Ticket should be valid when no event is associated");
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
            _mockEventRepository.Verify(e => e.GetById(It.IsAny<Guid>()), Times.Never,
                "Event repository should not be called when ticket has no event");
        }

        [TestMethod]
        public void ValidateTicket_WhenEnterTimeBeforeEventStart_ReturnsFalse()
        {
            Guid eventId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            DateTime eventDateStart = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime enterDate = new DateTime(2025, 1, 15, 9, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = eventDateStart.Date,
                EventId = eventId,
                Type = TicketType.EventSpecial
            };

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Morning Concert",
                Date = eventDateStart,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50
            };

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result, "Ticket should be invalid when trying to enter before event starts");
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
            _mockEventRepository.Verify(e => e.GetById(eventId), Times.Once);
        }

        [TestMethod]
        public void ValidateTicket_EventSpecialWithAttractionNotInEvent_ReturnsFalse()
        {
            Guid eventId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            Guid attractionInEventId = Guid.NewGuid();
            Guid attractionNotInEventId = Guid.NewGuid();
            DateTime eventDateStart = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime enterDate = new DateTime(2025, 1, 15, 11, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = eventDateStart.Date,
                EventId = eventId,
                Type = TicketType.EventSpecial
            };

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Concert",
                Date = eventDateStart,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50
            };

            Attraction attractionInEvent = new Attraction
            {
                Id = attractionInEventId,
                Name = "Main Stage"
            };

            eventEntity.AddAttraction(attractionInEvent);

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, Guid.NewGuid());

            Assert.IsFalse(result, "Ticket should be invalid when attraction does not belong to event");
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
            _mockEventRepository.Verify(e => e.GetById(eventId), Times.Once);
        }

        [TestMethod]
        public void ValidateTicket_EventSpecialWithAttractionInEvent_ReturnsTrue()
        {
            Guid eventId = Guid.NewGuid();
            Guid qrCode = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            DateTime eventDateStart = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime enterDate = new DateTime(2025, 1, 15, 11, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                QRCode = qrCode,
                VisitDate = eventDateStart.Date,
                EventId = eventId,
                Type = TicketType.EventSpecial
            };

            Attraction attraction = new Attraction
            {
                Id = attractionId,
                Name = "Main Stage"
            };

            EventAttraction eventAttraction = new EventAttraction
            {
                EventId = eventId,
                AttractionId = attraction.Id,
                Attraction = attraction
            };

            Event eventEntity = new Event
            {
                Id = eventId,
                Name = "Concert",
                Date = eventDateStart,
                MaxCapacity = 100,
                CurrentCapacity = 10,
                Cost = 50,
                Attractions = new List<EventAttraction> { eventAttraction }
            };

            eventEntity.AddAttraction(attraction);

            _mockTicketRepository.Setup(t => t.GetByQRCode(qrCode)).Returns(ticket);
            _mockEventRepository.Setup(e => e.GetById(eventId)).Returns(eventEntity);

            bool result = _ticketLogic.ValidateTicket(qrCode, null, enterDate, eventId, attractionId);

            Assert.IsTrue(result, "Ticket should be valid when attraction belongs to event");
            _mockTicketRepository.Verify(t => t.GetByQRCode(qrCode), Times.Once);
            _mockEventRepository.Verify(e => e.GetById(eventId), Times.Once);
        }
    }
}