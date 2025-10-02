using System;

namespace Domain
{
    public class VisitorReport
    {
        public DateTime Date { get; set; }
        public List<Report> Reports { get; set; }
        
        public VisitorReport(DateTime date, Report report)
        {
            Date = date;
            Reports = new List<Report>();
            Reports.Add(report);
        }
    }
}