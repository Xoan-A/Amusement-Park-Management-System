using DataAccess.Context;
using DataAccess.Repositories;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

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
        public void TestAddTicket()
        {
            Ticket ticket = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.General,
                QRCode = Guid.NewGuid()
            };

            Ticket addedTicket = _ticketRepository.Add(ticket);

            Assert.AreNotEqual(Guid.Empty, addedTicket.Id);
            Assert.AreEqual(_visitorId, addedTicket.VisitorId);
        }

        [TestMethod]
        public void TestGetById()
        {
            Guid eventId = Guid.NewGuid();
            Ticket ticket = new Ticket
            {
                VisitorId = _visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = eventId
            };

            Ticket addedTicket = _ticketRepository.Add(ticket);
            Ticket retrievedTicket = _ticketRepository.GetById(addedTicket.Id);

            Assert.AreEqual(addedTicket.Id, retrievedTicket.Id);
            Assert.AreEqual(_visitorId, retrievedTicket.VisitorId);
            Assert.AreEqual(TicketType.EventSpecial, retrievedTicket.Type);
            Assert.AreEqual(eventId, retrievedTicket.EventId);
        }

        [TestMethod]
        public void TestGetByVisitorId()
        {
            Guid eventId = Guid.NewGuid();
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
                EventId = eventId
            };

            _ticketRepository.Add(ticket1);
            _ticketRepository.Add(ticket2);

            IEnumerable<Ticket> visitorTickets = _ticketRepository.GetByVisitorId(_visitorId);

            Assert.AreEqual(2, visitorTickets.Count());
            Assert.IsTrue(visitorTickets.All(t => t.VisitorId == _visitorId));
        }

        [TestMethod]
        public void TestGetByQRCode()
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

            _ticketRepository.Add(ticket);
            Ticket retrievedTicket = _ticketRepository.GetByQRCode(qrCode);

            Assert.AreEqual(qrCode, retrievedTicket.QRCode);
        }

        [TestMethod]
        public void TestGetById_ReturnsNullForNonExistentId()
        {
            Ticket ticket = _ticketRepository.GetById(Guid.NewGuid());
            Assert.IsNull(ticket);
        }

        [TestMethod]
        public void TestGetByQRCode_ReturnsNullForNonExistentQR()
        {
            Ticket ticket = _ticketRepository.GetByQRCode(Guid.NewGuid());
            Assert.IsNull(ticket);
        }
    }
}