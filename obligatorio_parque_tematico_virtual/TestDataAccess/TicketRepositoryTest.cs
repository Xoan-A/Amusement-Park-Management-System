using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDataAccess
{
    [TestClass]
    public class TicketRepositoryTest
    {
        private AppDbContext _context;
        private ITicketRepository _ticketRepository;
        private Guid _visitorId;

        [TestInitialize]
        public void Setup()
        {
            DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Create a visitor for ticket tests
            _visitorId = Guid.NewGuid();
            User visitor = new User
            {
                Id = _visitorId,
                Name = "Test",
                LastName = "Visitor",
                Email = "testvisitor@test.com",
                Password = "password",
                BirthDate = new DateTime(1990, 1, 1)
            };
            _context.Users.Add(visitor);
            _context.SaveChanges();

            _ticketRepository = new TicketRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }

        [TestMethod]
        public async Task TestAddTicketAsync()
        {
            Ticket ticket = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            Ticket addedTicket = await _ticketRepository.AddAsync(ticket);

            Assert.IsNotNull(addedTicket);
            Assert.AreNotEqual(0, addedTicket.Id);
            Assert.AreEqual(_visitorId, addedTicket.VisitorId);
            Assert.AreEqual(TicketType.General, addedTicket.Type);
        }

        [TestMethod]
        public async Task TestGetByIdAsync()
        {
            Ticket ticket = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = 5
            };

            Ticket addedTicket = await _ticketRepository.AddAsync(ticket);
            Ticket retrievedTicket = await _ticketRepository.GetByIdAsync(addedTicket.Id);

            Assert.IsNotNull(retrievedTicket);
            Assert.AreEqual(addedTicket.Id, retrievedTicket.Id);
            Assert.AreEqual(_visitorId, retrievedTicket.VisitorId);
            Assert.AreEqual(TicketType.EventSpecial, retrievedTicket.Type);
            Assert.AreEqual(5, retrievedTicket.EventId);
        }

        [TestMethod]
        public async Task TestGetByVisitorIdAsync()
        {
            Ticket ticket1 = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            Ticket ticket2 = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(14),
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = 10
            };

            await _ticketRepository.AddAsync(ticket1);
            await _ticketRepository.AddAsync(ticket2);

            IEnumerable<Ticket> visitorTickets = await _ticketRepository.GetByVisitorIdAsync(_visitorId);

            Assert.IsNotNull(visitorTickets);
            Assert.AreEqual(2, visitorTickets.Count());
            Assert.IsTrue(visitorTickets.All(t => t.VisitorId == _visitorId));
        }

        [TestMethod]
        public async Task TestGetByQRCodeAsync()
        {
            Guid qrCode = Guid.NewGuid();
            Ticket ticket = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.General,
                QRCode = qrCode
            };

            await _ticketRepository.AddAsync(ticket);
            Ticket retrievedTicket = await _ticketRepository.GetByQRCodeAsync(qrCode);

            Assert.IsNotNull(retrievedTicket);
            Assert.AreEqual(qrCode, retrievedTicket.QRCode);
        }

        [TestMethod]
        public async Task TestGetByVisitDateAsync()
        {
            DateTime visitDate = DateTime.Now.Date.AddDays(7);

            Ticket ticket1 = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = visitDate,
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            Ticket ticket2 = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = visitDate,
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid()
            };

            Ticket ticket3 = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = visitDate.AddDays(1),
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            await _ticketRepository.AddAsync(ticket1);
            await _ticketRepository.AddAsync(ticket2);
            await _ticketRepository.AddAsync(ticket3);

            IEnumerable<Ticket> ticketsForDate = await _ticketRepository.GetByVisitDateAsync(visitDate);

            Assert.IsNotNull(ticketsForDate);
            Assert.AreEqual(2, ticketsForDate.Count());
            Assert.IsTrue(ticketsForDate.All(t => t.VisitDate.Date == visitDate.Date));
        }

        [TestMethod]
        public async Task TestGetByIdAsync_ReturnsNullForNonExistentId()
        {
            Ticket ticket = await _ticketRepository.GetByIdAsync(999);
            Assert.IsNull(ticket);
        }

        [TestMethod]
        public async Task TestGetByQRCodeAsync_ReturnsNullForNonExistentQR()
        {
            Ticket ticket = await _ticketRepository.GetByQRCodeAsync(Guid.NewGuid());
            Assert.IsNull(ticket);
        }
    }
}