using Domain;

namespace IDataAccess
{
    public interface IMaintenanceScheduleRepository
    {
        Task CreateAsync(MaintenanceSchedule schedule);
        Task<MaintenanceSchedule?> GetByIdAsync(Guid id);
        Task<List<MaintenanceSchedule>> GetAllAsync();
        Task<List<MaintenanceSchedule>> GetByAttractionIdAsync(Guid attractionId);
        Task<List<MaintenanceSchedule>> GetOverdueSchedulesAsync();
        Task<List<MaintenanceSchedule>> GetUpcomingSchedulesAsync(int daysAhead);
        Task UpdateAsync(MaintenanceSchedule schedule);
        Task DeleteAsync(Guid id);
    }
}
