using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<VisitorReport> VisitorReports { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
            UserRoles = new List<UserRole>();
            VisitorReports = new List<VisitorReport>();
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

        public void RegisterExit(Attraction attraction, DateTime exitDate)
        {
            VisitorReport visitorReport = VisitorReports.OrderByDescending(vr => vr.Date).FirstOrDefault();
            if (visitorReport == null)
                throw new ArgumentException("There is no report for the given enter date.");

            Report report =
                visitorReport.Reports.FirstOrDefault(r => r.ExitDate == null && r.Attraction.Id == attraction.Id);
            if (report == null)
                throw new ArgumentException(
                    "There is no report available to set an ExitDate regarding that attraction.");

            report.SetExitTime(exitDate);
        }
    }
}