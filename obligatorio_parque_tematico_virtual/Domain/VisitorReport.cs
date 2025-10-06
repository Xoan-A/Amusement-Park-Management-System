using System;

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
            Id = Guid.NewGuid();
            Date = date;
            Reports = new List<Report>();
            Reports.Add(report);
        }

        public VisitorReport()
        {
            Id = Guid.NewGuid();
            Reports = new List<Report>();
        }

        public void AddReport(Report report)
        {
            Reports.Add(report);
        }
    }
}