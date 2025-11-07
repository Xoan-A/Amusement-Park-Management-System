using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class MaintenanceLogic : IMaintenanceLogic
{
    private readonly IMaintenanceScheduleRepository _scheduleRepository;
    private readonly IMaintenanceRecordRepository _recordRepository;
    private readonly IAttractionRepository _attractionRepository;

    public MaintenanceLogic(
        IMaintenanceScheduleRepository scheduleRepository,
        IMaintenanceRecordRepository recordRepository,
        IAttractionRepository attractionRepository)
    {
        _scheduleRepository = scheduleRepository;
        _recordRepository = recordRepository;
        _attractionRepository = attractionRepository;
    }

    #region Schedule Management

    public async Task<Guid> CreateSchedule(MaintenanceScheduleRequest request, Guid createdBy)
    {
        Attraction attraction = await _attractionRepository.GetById(request.AttractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"Attraction with id {request.AttractionId} not found");
        }

        if (!Enum.TryParse<MaintenanceType>(request.MaintenanceType, out MaintenanceType maintenanceType))
        {
            throw new ArgumentException($"Invalid maintenance type: {request.MaintenanceType}");
        }

        MaintenanceSchedule schedule = new MaintenanceSchedule
        {
            Id = Guid.NewGuid(),
            AttractionId = request.AttractionId,
            ScheduledDate = request.ScheduledDate,
            MaintenanceType = maintenanceType,
            Description = request.Description,
            Status = MaintenanceStatus.Pending,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.CreateAsync(schedule);
        return schedule.Id;
    }

    public async Task<MaintenanceScheduleResponse> GetScheduleById(Guid id)
    {
        MaintenanceSchedule? schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {id} not found");
        }

        return MapToScheduleResponse(schedule);
    }

    public async Task<List<MaintenanceScheduleResponse>> GetAllSchedules()
    {
        List<MaintenanceSchedule> schedules = await _scheduleRepository.GetAllAsync();
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public async Task<List<MaintenanceScheduleResponse>> GetSchedulesByAttraction(Guid attractionId)
    {
        List<MaintenanceSchedule> schedules = await _scheduleRepository.GetByAttractionIdAsync(attractionId);
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public async Task<List<MaintenanceScheduleResponse>> GetOverdueSchedules()
    {
        List<MaintenanceSchedule> schedules = await _scheduleRepository.GetOverdueSchedulesAsync();
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public async Task<List<MaintenanceScheduleResponse>> GetUpcomingSchedules(int daysAhead)
    {
        List<MaintenanceSchedule> schedules = await _scheduleRepository.GetUpcomingSchedulesAsync(daysAhead);
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public async Task UpdateScheduleStatus(Guid id, string status)
    {
        MaintenanceSchedule? schedule = await _scheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {id} not found");
        }

        if (!Enum.TryParse<MaintenanceStatus>(status, out MaintenanceStatus maintenanceStatus))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }

        schedule.Status = maintenanceStatus;
        await _scheduleRepository.UpdateAsync(schedule);
    }

    public async Task DeleteSchedule(Guid id)
    {
        await _scheduleRepository.DeleteAsync(id);
    }

    #endregion

    #region Record Management

    public async Task<Guid> RecordMaintenance(MaintenanceRecordRequest request, Guid performedBy)
    {
        Attraction attraction = await _attractionRepository.GetById(request.AttractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"Attraction with id {request.AttractionId} not found");
        }

        if (request.MaintenanceScheduleId.HasValue)
        {
            MaintenanceSchedule? schedule = await _scheduleRepository.GetByIdAsync(request.MaintenanceScheduleId.Value);
            if (schedule == null)
            {
                throw new KeyNotFoundException($"Schedule with id {request.MaintenanceScheduleId} not found");
            }
        }

        if (!Enum.TryParse<MaintenanceType>(request.MaintenanceType, out MaintenanceType maintenanceType))
        {
            throw new ArgumentException($"Invalid maintenance type: {request.MaintenanceType}");
        }

        MaintenanceRecord record = new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            MaintenanceScheduleId = request.MaintenanceScheduleId,
            AttractionId = request.AttractionId,
            PerformedDate = request.PerformedDate,
            PerformedBy = performedBy,
            MaintenanceType = maintenanceType,
            Description = request.Description,
            Notes = request.Notes,
            Duration = request.Duration,
            CreatedAt = DateTime.UtcNow
        };

        await _recordRepository.CreateAsync(record);
        return record.Id;
    }

    public async Task<MaintenanceRecordResponse> GetRecordById(Guid id)
    {
        MaintenanceRecord? record = await _recordRepository.GetByIdAsync(id);
        if (record == null)
        {
            throw new KeyNotFoundException($"Record with id {id} not found");
        }

        return MapToRecordResponse(record);
    }

    public async Task<List<MaintenanceRecordResponse>> GetAllRecords()
    {
        List<MaintenanceRecord> records = await _recordRepository.GetAllAsync();
        return records.Select(MapToRecordResponse).ToList();
    }

    public async Task<List<MaintenanceRecordResponse>> GetRecordsByAttraction(Guid attractionId)
    {
        List<MaintenanceRecord> records = await _recordRepository.GetByAttractionIdAsync(attractionId);
        return records.Select(MapToRecordResponse).ToList();
    }

    public async Task<List<MaintenanceRecordResponse>> GetRecordsByOperator(Guid operatorId)
    {
        List<MaintenanceRecord> records = await _recordRepository.GetByOperatorAsync(operatorId);
        return records.Select(MapToRecordResponse).ToList();
    }

    public async Task<List<MaintenanceRecordResponse>> GetUnscheduledMaintenance()
    {
        List<MaintenanceRecord> records = await _recordRepository.GetUnscheduledMaintenanceAsync();
        return records.Select(MapToRecordResponse).ToList();
    }

    public async Task<List<MaintenanceRecordResponse>> GetMaintenanceHistory(Guid attractionId, DateTime dateFrom,
        DateTime dateTo)
    {
        List<MaintenanceRecord> records =
            await _recordRepository.GetByAttractionIdAndDateRangeAsync(attractionId, dateFrom, dateTo);
        return records.Select(MapToRecordResponse).ToList();
    }

    #endregion

    #region Business Operations

    public async Task<Guid> CompleteMaintenance(Guid scheduleId, MaintenanceRecordRequest recordRequest,
        Guid performedBy)
    {
        MaintenanceSchedule? schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {scheduleId} not found");
        }

        if (!schedule.CanComplete())
        {
            throw new ArgumentException(
                $"Schedule with id {scheduleId} cannot be completed (status: {schedule.Status})");
        }

        Attraction? attraction = await _attractionRepository.GetById(schedule.AttractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"Attraction with id {schedule.AttractionId} not found");
        }

        schedule.Status = MaintenanceStatus.Completed;
        await _scheduleRepository.UpdateAsync(schedule);

        recordRequest.MaintenanceScheduleId = scheduleId;
        recordRequest.AttractionId = schedule.AttractionId;
        Guid recordId = await RecordMaintenance(recordRequest, performedBy);

        return recordId;
    }

    #endregion

    #region Mapping Methods

    private MaintenanceScheduleResponse MapToScheduleResponse(MaintenanceSchedule schedule)
    {
        return new MaintenanceScheduleResponse
        {
            Id = schedule.Id,
            AttractionId = schedule.AttractionId,
            AttractionName = schedule.Attraction?.Name ?? "Unknown",
            ScheduledDate = schedule.ScheduledDate,
            MaintenanceType = schedule.MaintenanceType.ToString(),
            Description = schedule.Description,
            Status = schedule.Status.ToString(),
            CreatedAt = schedule.CreatedAt,
            IsOverdue = schedule.IsOverdue()
        };
    }

    private MaintenanceRecordResponse MapToRecordResponse(MaintenanceRecord record)
    {
        return new MaintenanceRecordResponse
        {
            Id = record.Id,
            MaintenanceScheduleId = record.MaintenanceScheduleId,
            AttractionId = record.AttractionId,
            AttractionName = record.Attraction?.Name ?? "Unknown",
            PerformedDate = record.PerformedDate,
            PerformedBy = record.PerformedBy,
            PerformedByName =
                record.Operator != null ? $"{record.Operator.Name} {record.Operator.LastName}" : "Unknown",
            MaintenanceType = record.MaintenanceType.ToString(),
            Description = record.Description,
            Notes = record.Notes,
            Duration = record.Duration,
            CreatedAt = record.CreatedAt
        };
    }

    #endregion
}