using Domain;

namespace IDataAccess
{
    public interface IMaintenanceScheduleRepository
    {
        void Create(MaintenanceSchedule schedule);
        MaintenanceSchedule? GetById(Guid id);
        List<MaintenanceSchedule> GetAll();
        List<MaintenanceSchedule> GetByAttractionId(Guid attractionId);
        List<MaintenanceSchedule> GetByStatus(MaintenanceStatus status);
        List<MaintenanceSchedule> GetOverdueSchedules();
        List<MaintenanceSchedule> GetByDateRange(DateTime dateFrom, DateTime dateTo);
        List<MaintenanceSchedule> GetUpcomingSchedules(int daysAhead);
        List<MaintenanceSchedule> GetByAttractionIdAndDateRange(Guid attractionId, DateTime dateFrom, DateTime dateTo);
        void Update(MaintenanceSchedule schedule);
        void Delete(Guid id);
    }
}
