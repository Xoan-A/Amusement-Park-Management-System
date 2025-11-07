using Domain;

namespace IDataAccess
{
    public interface IMaintenanceScheduleRepository
    {
        Task CreateAsync(MaintenanceSchedule schedule);
        Task<MaintenanceSchedule?> GetByIdAsync(Guid id);
        Task<List<MaintenanceSchedule>> GetAllAsync();
        Task<List<MaintenanceSchedule>> GetByAttractionIdAsync(Guid attractionId);
        Task<List<MaintenanceSchedule>> GetByStatusAsync(MaintenanceStatus status);
        Task<List<MaintenanceSchedule>> GetOverdueSchedulesAsync();
        Task<List<MaintenanceSchedule>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo);
        Task<List<MaintenanceSchedule>> GetUpcomingSchedulesAsync(int daysAhead);
        Task<List<MaintenanceSchedule>> GetByAttractionIdAndDateRangeAsync(Guid attractionId, DateTime dateFrom, DateTime dateTo);
        Task UpdateAsync(MaintenanceSchedule schedule);
        Task DeleteAsync(Guid id);
    }
}
