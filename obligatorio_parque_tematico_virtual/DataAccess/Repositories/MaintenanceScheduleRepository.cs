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

        public async Task CreateAsync(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Add(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<MaintenanceSchedule?> GetByIdAsync(Guid id)
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<MaintenanceSchedule>> GetAllAsync()
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceSchedule>> GetByAttractionIdAsync(Guid attractionId)
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.AttractionId == attractionId)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceSchedule>> GetByStatusAsync(MaintenanceStatus status)
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.Status == status)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceSchedule>> GetOverdueSchedulesAsync()
        {
            DateTime now = DateTime.Now;
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.ScheduledDate < now &&
                            (s.Status == MaintenanceStatus.Pending || s.Status == MaintenanceStatus.InProgress))
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceSchedule>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo)
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.ScheduledDate >= dateFrom && s.ScheduledDate <= dateTo)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceSchedule>> GetUpcomingSchedulesAsync(int daysAhead)
        {
            DateTime now = DateTime.Now;
            DateTime futureDate = now.AddDays(daysAhead);

            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.ScheduledDate >= now &&
                            s.ScheduledDate <= futureDate &&
                            s.Status == MaintenanceStatus.Pending)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceSchedule>> GetByAttractionIdAndDateRangeAsync(Guid attractionId, DateTime dateFrom,
            DateTime dateTo)
        {
            return await _context.MaintenanceSchedules
                .Include(s => s.Attraction)
                .Where(s => s.AttractionId == attractionId &&
                            s.ScheduledDate >= dateFrom &&
                            s.ScheduledDate <= dateTo)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();
        }

        public async Task UpdateAsync(MaintenanceSchedule schedule)
        {
            _context.MaintenanceSchedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            MaintenanceSchedule? schedule = await _context.MaintenanceSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.MaintenanceSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
        }
    }
}