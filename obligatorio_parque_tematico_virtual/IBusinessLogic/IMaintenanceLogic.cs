using Models.In;
using Models.Out;

namespace IBusinessLogic;

public interface IMaintenanceLogic
{
    Task<Guid> CreateSchedule(MaintenanceScheduleRequest request, Guid createdBy);
    Task<MaintenanceScheduleResponse> GetScheduleById(Guid id);
    Task<List<MaintenanceScheduleResponse>> GetAllSchedules();
    Task<List<MaintenanceScheduleResponse>> GetSchedulesByAttraction(Guid attractionId);
    Task<List<MaintenanceScheduleResponse>> GetOverdueSchedules();
    Task<List<MaintenanceScheduleResponse>> GetUpcomingSchedules(int daysAhead);
    Task UpdateScheduleStatus(Guid id, string status);
    Task DeleteSchedule(Guid id);

    Task<Guid> RecordMaintenance(MaintenanceRecordRequest request, Guid performedBy);
    Task<MaintenanceRecordResponse> GetRecordById(Guid id);
    Task<List<MaintenanceRecordResponse>> GetAllRecords();
    Task<List<MaintenanceRecordResponse>> GetRecordsByAttraction(Guid attractionId);
    Task<List<MaintenanceRecordResponse>> GetRecordsByOperator(Guid operatorId);
    Task<List<MaintenanceRecordResponse>> GetUnscheduledMaintenance();
    Task<List<MaintenanceRecordResponse>> GetMaintenanceHistory(Guid attractionId, DateTime dateFrom, DateTime dateTo);

    Task<Guid> CompleteMaintenance(Guid scheduleId, MaintenanceRecordRequest recordRequest, Guid performedBy);
}