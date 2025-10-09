namespace Domain
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid VisitorId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime VisitDate { get; set; }
        public TicketType Type { get; set; }
        public Guid QRCode { get; set; }
        public Guid? EventId { get; set; }
        public virtual User Visitor { get; set; }
    }
}