using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class MaintenanceScheduleRepository : IMaintenanceScheduleRepository
    {
        private readonly AppDbContext _context;

        public MaintenanceScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public void Create(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Add(schedule);
            _context.SaveChanges();
        }

        public MaintenanceSchedule? GetById(Guid id)
        {
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .FirstOrDefault(s => s.Id == id);
        }

        public List<MaintenanceSchedule> GetAll()
        {
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public List<MaintenanceSchedule> GetByAttractionId(Guid attractionId)
        {
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.AttractionId == attractionId)
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public List<MaintenanceSchedule> GetByStatus(MaintenanceStatus status)
        {
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.Status == status)
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public List<MaintenanceSchedule> GetOverdueSchedules()
        {
            DateTime now = DateTime.Now;
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.ScheduledDate < now &&
                            (s.Status == MaintenanceStatus.Pending || s.Status == MaintenanceStatus.InProgress))
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public List<MaintenanceSchedule> GetByDateRange(DateTime dateFrom, DateTime dateTo)
        {
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.ScheduledDate >= dateFrom && s.ScheduledDate <= dateTo)
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public List<MaintenanceSchedule> GetUpcomingSchedules(int daysAhead)
        {
            DateTime now = DateTime.Now;
            DateTime futureDate = now.AddDays(daysAhead);

            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.ScheduledDate >= now &&
                            s.ScheduledDate <= futureDate &&
                            s.Status == MaintenanceStatus.Pending)
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public List<MaintenanceSchedule> GetByAttractionIdAndDateRange(Guid attractionId, DateTime dateFrom,
            DateTime dateTo)
        {
            return _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.AttractionId == attractionId &&
                            s.ScheduledDate >= dateFrom &&
                            s.ScheduledDate <= dateTo)
                .OrderBy(s => s.ScheduledDate)
                .ToList();
        }

        public void Update(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Update(schedule);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            MaintenanceSchedule schedule = _context.MaintenanceSchedules.Find(id);
            if (schedule != null)
            {
                _context.MaintenanceSchedules.Remove(schedule);
                _context.SaveChanges();
            }
        }
    }
}