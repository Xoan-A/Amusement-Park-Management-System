using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class MaintenanceRecordRepository : IMaintenanceRecordRepository
    {
        private readonly AppDbContext _context;

        public MaintenanceRecordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(MaintenanceRecord record)
        {
            _context.MaintenanceRecords.Add(record);
            await _context.SaveChangesAsync();
        }

        public async Task<MaintenanceRecord?> GetByIdAsync(Guid id)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<MaintenanceRecord>> GetAllAsync()
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetByAttractionIdAsync(Guid attractionId)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.AttractionId == attractionId)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetByScheduleIdAsync(Guid scheduleId)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.MaintenanceScheduleId == scheduleId)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetUnscheduledMaintenanceAsync()
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.Operator)
                .Where(r => r.MaintenanceScheduleId == null)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetByOperatorAsync(Guid operatorId)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.PerformedBy == operatorId)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetByDateRangeAsync(DateTime dateFrom, DateTime dateTo)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.PerformedDate >= dateFrom && r.PerformedDate <= dateTo)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetByMaintenanceTypeAsync(MaintenanceType type)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.MaintenanceType == type)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task<List<MaintenanceRecord>> GetByAttractionIdAndDateRangeAsync(Guid attractionId, DateTime dateFrom,
            DateTime dateTo)
        {
            return await _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.AttractionId == attractionId &&
                            r.PerformedDate >= dateFrom &&
                            r.PerformedDate <= dateTo)
                .OrderByDescending(r => r.PerformedDate)
                .ToListAsync();
        }

        public async Task UpdateAsync(MaintenanceRecord record)
        {
            _context.MaintenanceRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            MaintenanceRecord? record = await _context.MaintenanceRecords.FindAsync(id);
            if (record != null)
            {
                _context.MaintenanceRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}