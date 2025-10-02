namespace Domain;

public class Report
{
    public DateTime EnterDate { get; set; }
    public DateTime? ExitDate { get; set; }
    public Attraction Attraction { get; set; }
    
    public Report(DateTime enterDate, Attraction attraction)
    {
        EnterDate = enterDate;
        Attraction = attraction;
    }

    public void SetExitTime(DateTime date)
    {
        if (date < EnterDate)
            throw new ArgumentException("Enter date must be greater than or equal to EnterDate");
        
        ExitDate = date;
    }
}