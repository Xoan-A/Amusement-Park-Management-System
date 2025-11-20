using Domain;

namespace IDataAccess
{
    public interface IMaintenanceScheduleRepository
    {
        void Create(MaintenanceSchedule schedule);
        MaintenanceSchedule? GetById(Guid id);
        List<MaintenanceSchedule> GetAll();
        List<MaintenanceSchedule> GetByAttractionId(Guid attractionId);
        List<MaintenanceSchedule> GetOverdueSchedules();
        List<MaintenanceSchedule> GetUpcomingSchedules(int daysAhead);
        void Update(MaintenanceSchedule schedule);
        void Delete(Guid id);
    }
}
