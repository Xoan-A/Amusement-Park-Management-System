namespace Domain
{
    public class VisitorReport
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public Guid VisitorId { get; set; }
        public User Visitor { get; set; }
        public List<Report> Reports { get; set; }

        public VisitorReport(DateTime date, Report report)
        {
            Date = date;
            Reports = new List<Report>();
            Reports.Add(report);
        }

        public VisitorReport()
        {
            Reports = new List<Report>();
        }

        public void AddReport(Report report)
        {
            Reports.Add(report);
        }
    }
}