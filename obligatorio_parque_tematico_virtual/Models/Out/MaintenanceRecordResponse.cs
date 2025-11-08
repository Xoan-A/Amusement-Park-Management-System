namespace Models.Out;

public class MaintenanceRecordResponse
{
    public Guid Id { get; set; }
    public Guid? MaintenanceScheduleId { get; set; }
    public Guid AttractionId { get; set; }
    public string AttractionName { get; set; }
    public DateTime PerformedDate { get; set; }
    public Guid PerformedBy { get; set; }
    public string PerformedByName { get; set; }
    public string Description { get; set; }
    public string? Notes { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; }
}
