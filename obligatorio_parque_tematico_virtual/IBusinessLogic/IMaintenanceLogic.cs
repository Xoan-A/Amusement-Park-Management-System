using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IMaintenanceLogic
{
    Guid CreateSchedule(MaintenanceScheduleRequest request);
    MaintenanceScheduleResponse GetScheduleById(Guid id);
    List<MaintenanceScheduleResponse> GetAllSchedules();
    List<MaintenanceScheduleResponse> GetSchedulesByAttraction(Guid attractionId);
    List<MaintenanceScheduleResponse> GetOverdueSchedules();
    List<MaintenanceScheduleResponse> GetUpcomingSchedules(int daysAhead);
    void UpdateScheduleStatus(Guid id, string status);
    void DeleteSchedule(Guid id);

    Guid CompleteMaintenance(Guid scheduleId, Guid performedBy);
}