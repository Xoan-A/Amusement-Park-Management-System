namespace Domain;

public class DateTimeConfiguration
{
    public int Id { get; set; }
    public DateTime CurrentDateTime { get; set; }

    public DateTimeConfiguration(DateTime currentDateTime)
    {
        CurrentDateTime = currentDateTime;
    }
}