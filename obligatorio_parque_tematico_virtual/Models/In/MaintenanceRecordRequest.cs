namespace Models.In;

public class MaintenanceRecordRequest
{
    public Guid? MaintenanceScheduleId { get; set; }
    public Guid AttractionId { get; set; }
    public DateTime PerformedDate { get; set; }
    public string Description { get; set; }
    public string? Notes { get; set; }
    public TimeSpan Duration { get; set; }
}
