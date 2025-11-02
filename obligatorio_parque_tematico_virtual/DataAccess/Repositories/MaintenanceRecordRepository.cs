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

        public void Create(MaintenanceRecord record)
        {
            _context.MaintenanceRecords.Add(record);
            _context.SaveChanges();
        }

        public MaintenanceRecord? GetById(Guid id)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .FirstOrDefault(r => r.Id == id);
        }

        public List<MaintenanceRecord> GetAll()
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetByAttractionId(Guid attractionId)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.AttractionId == attractionId)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetByScheduleId(Guid scheduleId)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.MaintenanceScheduleId == scheduleId)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetUnscheduledMaintenance()
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.Operator)
                .Where(r => r.MaintenanceScheduleId == null)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetByOperator(Guid operatorId)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.PerformedBy == operatorId)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetByDateRange(DateTime dateFrom, DateTime dateTo)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.PerformedDate >= dateFrom && r.PerformedDate <= dateTo)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetByMaintenanceType(MaintenanceType type)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.MaintenanceType == type)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public List<MaintenanceRecord> GetByAttractionIdAndDateRange(Guid attractionId, DateTime dateFrom, DateTime dateTo)
        {
            return _context.MaintenanceRecords
                .Include(r => r.Attraction)
                .Include(r => r.MaintenanceSchedule)
                .Include(r => r.Operator)
                .Where(r => r.AttractionId == attractionId &&
                           r.PerformedDate >= dateFrom &&
                           r.PerformedDate <= dateTo)
                .OrderByDescending(r => r.PerformedDate)
                .ToList();
        }

        public void Update(MaintenanceRecord record)
        {
            _context.MaintenanceRecords.Update(record);
            _context.SaveChanges();
        }

        public void Delete(Guid id)
        {
            var record = _context.MaintenanceRecords.Find(id);
            if (record != null)
            {
                _context.MaintenanceRecords.Remove(record);
                _context.SaveChanges();
            }
        }
    }
}
