using Domain;

namespace TestDomain
{
    [TestClass]
    public class TicketTest
    {
        [TestMethod]
        public void TestTicketCreation()
        {
            DateTime purchaseDate = new DateTime(2025, 1, 1, 10, 0, 0);
            DateTime visitDate = new DateTime(2025, 1, 15, 0, 0, 0);
            Guid qrCode = Guid.NewGuid();
            Guid visitorId = Guid.NewGuid();
            Guid ticketId = Guid.NewGuid();

            Ticket ticket = new Ticket
            {
                Id = ticketId,
                VisitorId = visitorId,
                PurchaseDate = purchaseDate,
                VisitDate = visitDate,
                Type = TicketType.General,
                QRCode = qrCode
            };

            Assert.AreEqual(ticketId, ticket.Id);
            Assert.AreEqual(visitorId, ticket.VisitorId);
            Assert.AreEqual(purchaseDate, ticket.PurchaseDate);
            Assert.AreEqual(visitDate, ticket.VisitDate);
            Assert.AreEqual(TicketType.General, ticket.Type);
            Assert.AreEqual(qrCode, ticket.QRCode);
            Assert.IsNull(ticket.EventId);
        }

        [TestMethod]
        public void TestTicketWithEventId()
        {
            Guid visitorId = Guid.NewGuid();
            Guid ticketId = Guid.NewGuid();
            Guid eventId = Guid.NewGuid();
            Ticket ticket = new Ticket
            {
                Id = ticketId,
                VisitorId = visitorId,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(5),
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = eventId
            };

            Assert.AreEqual(ticketId, ticket.Id);
            Assert.AreEqual(visitorId, ticket.VisitorId);
            Assert.AreEqual(TicketType.EventSpecial, ticket.Type);
            Assert.AreEqual(eventId, ticket.EventId);
        }

        [TestMethod]
        public void TestTicketTypeEnum()
        {
            TicketType generalType = TicketType.General;
            TicketType eventType = TicketType.EventSpecial;

            Assert.AreEqual(TicketType.General, generalType);
            Assert.AreEqual(TicketType.EventSpecial, eventType);
            Assert.AreNotEqual(generalType, eventType);
        }

        [TestMethod]
        public void TestTicketWithVisitorRelationship()
        {
            Guid visitorGuid = Guid.NewGuid();
            Guid ticketId = Guid.NewGuid();
            User visitor = new User
            {
                Id = visitorGuid,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "hashedpassword",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard
            };

            Ticket ticket = new Ticket
            {
                Id = ticketId,
                VisitorId = visitor.Id,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.General,
                QRCode = Guid.NewGuid(),
                Visitor = visitor
            };

            Assert.AreEqual(visitorGuid, ticket.VisitorId);
            Assert.AreEqual("John", ticket.Visitor.Name);
        }

        [TestMethod]
        public void TestQRCodeUniqueness()
        {
            Guid qrCode1 = Guid.NewGuid();
            Guid qrCode2 = Guid.NewGuid();

            Ticket ticket1 = new Ticket { QRCode = qrCode1 };
            Ticket ticket2 = new Ticket { QRCode = qrCode2 };

            Assert.AreNotEqual(ticket1.QRCode, ticket2.QRCode);
            Assert.AreNotEqual(Guid.Empty, ticket1.QRCode);
            Assert.AreNotEqual(Guid.Empty, ticket2.QRCode);
        }
    }
}