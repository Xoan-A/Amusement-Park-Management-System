namespace Domain;

public class Report
{
    public Guid Id { get; set; }
    public DateTime EnterDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public Guid AttractionId { get; set; }
    public Attraction Attraction { get; set; }
    public Guid VisitorReportId { get; set; }
    public VisitorReport VisitorReport { get; set; }

    public Report(DateTime enterDate, Attraction attraction)
    {
        Id = Guid.NewGuid();
        EnterDate = enterDate;
        Attraction = attraction;
    }

    public Report()
    {
        Id = Guid.NewGuid();
    }

    public void SetExitTime(DateTime date)
    {
        if (date < EnterDate)
            throw new ArgumentException("Enter date must be greater than or equal to EnterDate");

        ExitDate = date;
    }
}