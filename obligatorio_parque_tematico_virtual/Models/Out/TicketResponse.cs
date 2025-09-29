using System;
using Domain;

namespace Models.Out
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public Guid VisitorId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime VisitDate { get; set; }
        public TicketType Type { get; set; }
        public Guid QRCode { get; set; }
        public int? EventId { get; set; }
    }
}