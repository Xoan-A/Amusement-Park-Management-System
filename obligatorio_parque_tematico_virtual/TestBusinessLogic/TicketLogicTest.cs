using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic;
using Domain;
using IBusinessLogic;
using IDataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestBusinessLogic
{
    [TestClass]
    public class TicketLogicTest
    {
        private Mock<ITicketRepository> _mockTicketRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IDateTimeLogic> _mockDateTimeLogic;
        private ITicketLogic _ticketLogic;

        [TestInitialize]
        public void Setup()
        {
            _mockTicketRepository = new Mock<ITicketRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockDateTimeLogic = new Mock<IDateTimeLogic>();
            _ticketLogic = new TicketLogic(_mockTicketRepository.Object, _mockUserRepository.Object, _mockDateTimeLogic.Object);
        }

        [TestMethod]
        public async Task TestPurchaseTicketAsync_Success()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 15, 0, 0, 0);

            Visitor visitor = new Visitor
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword"
            };

            Ticket expectedTicket = new Ticket
            {
                Id = 1,
                VisitorId = visitorId,
                PurchaseDate = currentDate,
                VisitDate = visitDate,
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);
            _mockTicketRepository.Setup(t => t.AddAsync(It.IsAny<Ticket>())).ReturnsAsync(expectedTicket);

            Ticket result = await _ticketLogic.PurchaseTicketAsync(visitorId, visitDate, TicketType.General, null);

            Assert.IsNotNull(result);
            Assert.AreEqual(visitorId, result.VisitorId);
            Assert.AreEqual(visitDate, result.VisitDate);
            Assert.AreEqual(TicketType.General, result.Type);
            Assert.AreNotEqual(Guid.Empty, result.QRCode);
            _mockTicketRepository.Verify(t => t.AddAsync(It.IsAny<Ticket>()), Times.Once);
        }

        [TestMethod]
        public async Task TestPurchaseTicketAsync_VisitorNotFound()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime visitDate = DateTime.Now.AddDays(7);

            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns((User)null);

            Ticket result = await _ticketLogic.PurchaseTicketAsync(visitorId, visitDate, TicketType.General, null);

            Assert.IsNull(result);
            _mockTicketRepository.Verify(t => t.AddAsync(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public async Task TestPurchaseTicketAsync_PastVisitDate()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 15, 10, 0, 0);
            DateTime pastVisitDate = new DateTime(2025, 1, 10, 0, 0, 0);

            Visitor visitor = new Visitor
            {
                Id = visitorId,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com"
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);

            Ticket result = await _ticketLogic.PurchaseTicketAsync(visitorId, pastVisitDate, TicketType.General, null);

            Assert.IsNull(result);
            _mockTicketRepository.Verify(t => t.AddAsync(It.IsAny<Ticket>()), Times.Never);
        }

        [TestMethod]
        public async Task TestPurchaseTicketAsync_EventSpecialType()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime currentDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 20, 0, 0, 0);
            int eventId = 5;

            Visitor visitor = new Visitor
            {
                Id = visitorId,
                Name = "Jane",
                LastName = "Smith"
            };

            Ticket expectedTicket = new Ticket
            {
                Id = 2,
                VisitorId = visitorId,
                PurchaseDate = currentDate,
                VisitDate = visitDate,
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = eventId
            };

            _mockDateTimeLogic.Setup(d => d.GetCurrentDateTime()).Returns(currentDate);
            _mockUserRepository.Setup(u => u.GetById(visitorId)).Returns(visitor);
            _mockTicketRepository.Setup(t => t.AddAsync(It.IsAny<Ticket>())).ReturnsAsync(expectedTicket);

            Ticket result = await _ticketLogic.PurchaseTicketAsync(visitorId, visitDate, TicketType.EventSpecial, eventId);

            Assert.IsNotNull(result);
            Assert.AreEqual(TicketType.EventSpecial, result.Type);
            Assert.AreEqual(eventId, result.EventId);
        }

        [TestMethod]
        public async Task TestGetTicketByIdAsync()
        {
            int ticketId = 123;
            Ticket expectedTicket = new Ticket
            {
                Id = ticketId,
                VisitorId = Guid.NewGuid(),
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByIdAsync(ticketId)).ReturnsAsync(expectedTicket);

            Ticket result = await _ticketLogic.GetTicketByIdAsync(ticketId);

            Assert.IsNotNull(result);
            Assert.AreEqual(ticketId, result.Id);
            _mockTicketRepository.Verify(t => t.GetByIdAsync(ticketId), Times.Once);
        }

        [TestMethod]
        public async Task TestGetVisitorTicketsAsync()
        {
            Guid visitorId = Guid.NewGuid();
            List<Ticket> expectedTickets = new List<Ticket>
            {
                new Ticket { Id = 1, VisitorId = visitorId, Type = TicketType.General },
                new Ticket { Id = 2, VisitorId = visitorId, Type = TicketType.EventSpecial }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(expectedTickets);

            IEnumerable<Ticket> result = await _ticketLogic.GetVisitorTicketsAsync(visitorId);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(t => t.VisitorId == visitorId));
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task TestGetTicketByQRCodeAsync()
        {
            Guid qrCode = Guid.NewGuid();
            Ticket expectedTicket = new Ticket
            {
                Id = 456,
                QRCode = qrCode,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCodeAsync(qrCode)).ReturnsAsync(expectedTicket);

            Ticket result = await _ticketLogic.GetTicketByQRCodeAsync(qrCode);

            Assert.IsNotNull(result);
            Assert.AreEqual(qrCode, result.QRCode);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(qrCode), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_BothQrAndNfcNull_ReturnsFalse()
        {
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int? eventId = null;

            bool result = await _ticketLogic.ValidateTicketAsync(null, null, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(It.IsAny<Guid>()), Times.Never);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidQrWithMatchingDateAndEventId_ReturnsTrue()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int eventId = 5;

            Ticket ticket = new Ticket
            {
                Id = 1,
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                EventId = eventId,
                Type = TicketType.EventSpecial
            };

            _mockTicketRepository.Setup(t => t.GetByQRCodeAsync(qrCode)).ReturnsAsync(ticket);

            bool result = await _ticketLogic.ValidateTicketAsync(qrCode, null, enterDate, eventId);

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(qrCode), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidQrWithMatchingDateButMismatchingEventId_ReturnsFalse()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int eventId = 5;

            Ticket ticket = new Ticket
            {
                Id = 1,
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                EventId = 10,
                Type = TicketType.EventSpecial
            };

            _mockTicketRepository.Setup(t => t.GetByQRCodeAsync(qrCode)).ReturnsAsync(ticket);

            bool result = await _ticketLogic.ValidateTicketAsync(qrCode, null, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(qrCode), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidQrWithMismatchingDate_ReturnsFalse()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int? eventId = null;

            Ticket ticket = new Ticket
            {
                Id = 1,
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 20, 0, 0, 0),
                EventId = eventId,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCodeAsync(qrCode)).ReturnsAsync(ticket);

            bool result = await _ticketLogic.ValidateTicketAsync(qrCode, null, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(qrCode), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_QrCodeNotFound_ReturnsFalse()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int? eventId = null;

            _mockTicketRepository.Setup(t => t.GetByQRCodeAsync(qrCode)).ReturnsAsync((Ticket)null);

            bool result = await _ticketLogic.ValidateTicketAsync(qrCode, null, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(qrCode), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidNfcWithMatchingDateAndEventId_ReturnsTrue()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int eventId = 5;

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = eventId,
                    Type = TicketType.EventSpecial
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(tickets);

            bool result = await _ticketLogic.ValidateTicketAsync(null, visitorId, enterDate, eventId);

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidNfcWithMatchingDateButMismatchingEventId_ReturnsFalse()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int eventId = 5;

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = 10,
                    Type = TicketType.EventSpecial
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(tickets);

            bool result = await _ticketLogic.ValidateTicketAsync(null, visitorId, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidNfcWithMismatchingDate_ReturnsFalse()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int? eventId = null;

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 20, 0, 0, 0),
                    EventId = eventId,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(tickets);

            bool result = await _ticketLogic.ValidateTicketAsync(null, visitorId, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_NfcWithNoTickets_ReturnsFalse()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int? eventId = null;

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(new List<Ticket>());

            bool result = await _ticketLogic.ValidateTicketAsync(null, visitorId, enterDate, eventId);

            Assert.IsFalse(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidQrWithNullEventId_ReturnsTrue()
        {
            Guid qrCode = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);

            Ticket ticket = new Ticket
            {
                Id = 1,
                QRCode = qrCode,
                VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                EventId = null,
                Type = TicketType.General
            };

            _mockTicketRepository.Setup(t => t.GetByQRCodeAsync(qrCode)).ReturnsAsync(ticket);

            bool result = await _ticketLogic.ValidateTicketAsync(qrCode, null, enterDate, null);

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByQRCodeAsync(qrCode), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_ValidNfcWithNullEventId_ReturnsTrue()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = null,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(tickets);

            bool result = await _ticketLogic.ValidateTicketAsync(null, visitorId, enterDate, null);

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }

        [TestMethod]
        public async Task TestValidateTicketAsync_NfcWithMultipleTicketsFindsCorrectOne_ReturnsTrue()
        {
            Guid visitorId = Guid.NewGuid();
            DateTime enterDate = new DateTime(2025, 1, 15, 10, 0, 0);
            int eventId = 5;

            List<Ticket> tickets = new List<Ticket>
            {
                new Ticket
                {
                    Id = 1,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 10, 0, 0, 0),
                    EventId = 3,
                    Type = TicketType.General
                },
                new Ticket
                {
                    Id = 2,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 15, 0, 0, 0),
                    EventId = eventId,
                    Type = TicketType.EventSpecial
                },
                new Ticket
                {
                    Id = 3,
                    VisitorId = visitorId,
                    VisitDate = new DateTime(2025, 1, 20, 0, 0, 0),
                    EventId = 7,
                    Type = TicketType.General
                }
            };

            _mockTicketRepository.Setup(t => t.GetByVisitorIdAsync(visitorId)).ReturnsAsync(tickets);

            bool result = await _ticketLogic.ValidateTicketAsync(null, visitorId, enterDate, eventId);

            Assert.IsTrue(result);
            _mockTicketRepository.Verify(t => t.GetByVisitorIdAsync(visitorId), Times.Once);
        }
    }
}