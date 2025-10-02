using System;

namespace Domain
{
    public abstract class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<VisitorReport> VisitorReports { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
        }

        public void RegisterEntry(Attraction attraction, DateTime enterDate)
        {
            Report report = new Report(enterDate, attraction);
            VisitorReport visitorReport = VisitorReports.FirstOrDefault(vr => vr.Date.Date == enterDate.Date);

            if (visitorReport == null)
            {
                visitorReport = new VisitorReport(enterDate, report);
                VisitorReports.Add(visitorReport);
            }
            else
                visitorReport.AddReport(report);
        }
        
        public void RegisterExit(DateTime enterDate, DateTime exitDate)
        {
            VisitorReport visitorReport = VisitorReports.FirstOrDefault(vr => vr.Date.Date == enterDate.Date);
            if (visitorReport == null)
                throw new ArgumentException("There is no report for the given enter date.");
            
            Report report = visitorReport.Reports.FirstOrDefault(r => r.EnterDate == enterDate);
            if (report == null)
                throw new ArgumentException("There is no report with the given enter date.");
            
            report.SetExitTime(exitDate);
        }
    }
}