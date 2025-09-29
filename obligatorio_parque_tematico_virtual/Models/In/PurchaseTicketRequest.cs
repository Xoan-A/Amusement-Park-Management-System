using System;
using Domain;

namespace Models.In
{
    public class PurchaseTicketRequest
    {
        public Guid VisitorId { get; set; }
        public DateTime VisitDate { get; set; }
        public TicketType TicketType { get; set; }
        public int? EventId { get; set; }
    }
}