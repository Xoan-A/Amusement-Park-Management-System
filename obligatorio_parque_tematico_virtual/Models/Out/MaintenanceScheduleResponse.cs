namespace Models.Out;

public class MaintenanceScheduleResponse
{
    public Guid Id { get; set; }
    public Guid AttractionId { get; set; }
    public string AttractionName { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOverdue { get; set; }
}
