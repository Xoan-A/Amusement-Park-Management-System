using Domain;

namespace IDataAccess
{
    public interface IMaintenanceRecordRepository
    {
        void Create(MaintenanceRecord record);
        MaintenanceRecord? GetById(Guid id);
        List<MaintenanceRecord> GetAll();
        List<MaintenanceRecord> GetByAttractionId(Guid attractionId);
        List<MaintenanceRecord> GetByScheduleId(Guid scheduleId);
        List<MaintenanceRecord> GetUnscheduledMaintenance();
        List<MaintenanceRecord> GetByOperator(Guid operatorId);
        List<MaintenanceRecord> GetByDateRange(DateTime dateFrom, DateTime dateTo);
        List<MaintenanceRecord> GetByMaintenanceType(MaintenanceType type);
        List<MaintenanceRecord> GetByAttractionIdAndDateRange(Guid attractionId, DateTime dateFrom, DateTime dateTo);
        void Update(MaintenanceRecord record);
        void Delete(Guid id);
    }
}
