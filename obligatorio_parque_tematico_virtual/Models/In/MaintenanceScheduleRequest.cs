namespace Models.In;

public class MaintenanceScheduleRequest
{
    public Guid AttractionId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string MaintenanceType { get; set; }
    public string Description { get; set; }
}
