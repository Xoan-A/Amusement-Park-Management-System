namespace Models.Out
{
    public class TicketResponse
    {
        public Guid Id { get; set; }
        public Guid VisitorId { get; set; }
        public string VisitorName { get; set; }
        public string VisitorLastName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime VisitDate { get; set; }
        public int Type { get; set; }
        public Guid QRCode { get; set; }
        public Guid? EventId { get; set; }
    }
}