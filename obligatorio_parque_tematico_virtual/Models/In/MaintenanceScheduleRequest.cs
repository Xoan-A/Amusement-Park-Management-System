namespace Models.In;

public class MaintenanceScheduleRequest
{
    public Guid AttractionId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Description { get; set; }
    public int EstimatedDuration { get; set; }
}
