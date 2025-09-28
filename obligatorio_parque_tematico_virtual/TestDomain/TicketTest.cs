using System;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Ticket ticket = new Ticket
            {
                Id = 1,
                VisitorId = 123,
                PurchaseDate = purchaseDate,
                VisitDate = visitDate,
                Type = TicketType.General,
                QRCode = qrCode
            };

            Assert.AreEqual(1, ticket.Id);
            Assert.AreEqual(123, ticket.VisitorId);
            Assert.AreEqual(purchaseDate, ticket.PurchaseDate);
            Assert.AreEqual(visitDate, ticket.VisitDate);
            Assert.AreEqual(TicketType.General, ticket.Type);
            Assert.AreEqual(qrCode, ticket.QRCode);
            Assert.IsNull(ticket.EventId);
        }

        [TestMethod]
        public void TestTicketWithEventId()
        {
            Ticket ticket = new Ticket
            {
                Id = 2,
                VisitorId = 456,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(5),
                Type = TicketType.EventSpecial,
                QRCode = Guid.NewGuid(),
                EventId = 10
            };

            Assert.AreEqual(2, ticket.Id);
            Assert.AreEqual(456, ticket.VisitorId);
            Assert.AreEqual(TicketType.EventSpecial, ticket.Type);
            Assert.AreEqual(10, ticket.EventId);
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
            Visitor visitor = new Visitor
            {
                Id = 1,
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                BirthDate = new DateTime(1990, 1, 1),
                MembershipLevel = MembershipLevel.Standard
            };

            Ticket ticket = new Ticket
            {
                Id = 3,
                VisitorId = visitor.Id,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = TicketType.General,
                QRCode = Guid.NewGuid(),
                Visitor = visitor
            };

            Assert.IsNotNull(ticket.Visitor);
            Assert.AreEqual(visitor.Id, ticket.VisitorId);
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