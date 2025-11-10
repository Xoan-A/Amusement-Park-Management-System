using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IMaintenanceLogic
{
    Task<Guid> CreateSchedule(MaintenanceScheduleRequest request);
    Task<MaintenanceScheduleResponse> GetScheduleById(Guid id);
    Task<List<MaintenanceScheduleResponse>> GetAllSchedules();
    Task<List<MaintenanceScheduleResponse>> GetSchedulesByAttraction(Guid attractionId);
    Task<List<MaintenanceScheduleResponse>> GetOverdueSchedules();
    Task<List<MaintenanceScheduleResponse>> GetUpcomingSchedules(int daysAhead);
    Task UpdateScheduleStatus(Guid id, string status);
    Task DeleteSchedule(Guid id);

    Task<Guid> CompleteMaintenance(Guid scheduleId, Guid performedBy);
}