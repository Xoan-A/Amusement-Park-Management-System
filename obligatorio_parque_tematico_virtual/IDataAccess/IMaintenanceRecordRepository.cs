using Domain;

namespace IDataAccess
{
    public interface IMaintenanceRecordRepository
    {
        Task CreateAsync(MaintenanceRecord record);
        Task<MaintenanceRecord?> GetByIdAsync(Guid id);
        Task<List<MaintenanceRecord>> GetAllAsync();
        Task<List<MaintenanceRecord>> GetByAttractionIdAsync(Guid attractionId);
        Task<List<MaintenanceRecord>> GetByScheduleIdAsync(Guid scheduleId);
        Task<List<MaintenanceRecord>> GetUnscheduledMaintenanceAsync();
        Task<List<MaintenanceRecord>> GetByOperatorAsync(Guid operatorId);
        Task<List<MaintenanceRecord>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo);
        Task<List<MaintenanceRecord>> GetByMaintenanceTypeAsync(MaintenanceType type);
        Task<List<MaintenanceRecord>> GetByAttractionIdAndDateRangeAsync(Guid attractionId, DateTime dateFrom, DateTime dateTo);
        Task UpdateAsync(MaintenanceRecord record);
        Task DeleteAsync(Guid id);
    }
}
