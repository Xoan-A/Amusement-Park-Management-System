namespace Models.In
{
    public class PurchaseTicketRequest
    {
        public Guid VisitorId { get; set; }
        public DateTime VisitDate { get; set; }
        public int TicketType { get; set; }
        public Guid? EventId { get; set; }
    }
}