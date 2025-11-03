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

        _scheduleRepository.Create(schedule);
        return schedule.Id;
    }

    public async Task<MaintenanceScheduleResponse> GetScheduleById(Guid id)
    {
        MaintenanceSchedule schedule = _scheduleRepository.GetById(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {id} not found");
        }

        return await Task.FromResult(MapToScheduleResponse(schedule));
    }

    public async Task<List<MaintenanceScheduleResponse>> GetAllSchedules()
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetAll();
        return await Task.FromResult(schedules.Select(MapToScheduleResponse).ToList());
    }

    public async Task<List<MaintenanceScheduleResponse>> GetSchedulesByAttraction(Guid attractionId)
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetByAttractionId(attractionId);
        return await Task.FromResult(schedules.Select(MapToScheduleResponse).ToList());
    }

    public async Task<List<MaintenanceScheduleResponse>> GetOverdueSchedules()
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetOverdueSchedules();
        return await Task.FromResult(schedules.Select(MapToScheduleResponse).ToList());
    }

    public async Task<List<MaintenanceScheduleResponse>> GetUpcomingSchedules(int daysAhead)
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetUpcomingSchedules(daysAhead);
        return await Task.FromResult(schedules.Select(MapToScheduleResponse).ToList());
    }

    public async Task UpdateScheduleStatus(Guid id, string status)
    {
        MaintenanceSchedule schedule = _scheduleRepository.GetById(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {id} not found");
        }

        if (!Enum.TryParse<MaintenanceStatus>(status, out MaintenanceStatus maintenanceStatus))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }

        schedule.Status = maintenanceStatus;
        _scheduleRepository.Update(schedule);
        await Task.CompletedTask;
    }

    public async Task DeleteSchedule(Guid id)
    {
        _scheduleRepository.Delete(id);
        await Task.CompletedTask;
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
            MaintenanceSchedule schedule = _scheduleRepository.GetById(request.MaintenanceScheduleId.Value);
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

        _recordRepository.Create(record);
        return record.Id;
    }

    public async Task<MaintenanceRecordResponse> GetRecordById(Guid id)
    {
        MaintenanceRecord record = _recordRepository.GetById(id);
        if (record == null)
        {
            throw new KeyNotFoundException($"Record with id {id} not found");
        }

        return await Task.FromResult(MapToRecordResponse(record));
    }

    public async Task<List<MaintenanceRecordResponse>> GetAllRecords()
    {
        List<MaintenanceRecord> records = _recordRepository.GetAll();
        return await Task.FromResult(records.Select(MapToRecordResponse).ToList());
    }

    public async Task<List<MaintenanceRecordResponse>> GetRecordsByAttraction(Guid attractionId)
    {
        List<MaintenanceRecord> records = _recordRepository.GetByAttractionId(attractionId);
        return await Task.FromResult(records.Select(MapToRecordResponse).ToList());
    }

    public async Task<List<MaintenanceRecordResponse>> GetRecordsByOperator(Guid operatorId)
    {
        List<MaintenanceRecord> records = _recordRepository.GetByOperator(operatorId);
        return await Task.FromResult(records.Select(MapToRecordResponse).ToList());
    }

    public async Task<List<MaintenanceRecordResponse>> GetUnscheduledMaintenance()
    {
        List<MaintenanceRecord> records = _recordRepository.GetUnscheduledMaintenance();
        return await Task.FromResult(records.Select(MapToRecordResponse).ToList());
    }

    public async Task<List<MaintenanceRecordResponse>> GetMaintenanceHistory(Guid attractionId, DateTime dateFrom,
        DateTime dateTo)
    {
        List<MaintenanceRecord> records =
            _recordRepository.GetByAttractionIdAndDateRange(attractionId, dateFrom, dateTo);
        return await Task.FromResult(records.Select(MapToRecordResponse).ToList());
    }

    #endregion

    #region Business Operations

    public async Task<Guid> CompleteMaintenance(Guid scheduleId, MaintenanceRecordRequest recordRequest,
        Guid performedBy)
    {
        MaintenanceSchedule schedule = _scheduleRepository.GetById(scheduleId);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {scheduleId} not found");
        }

        if (!schedule.CanComplete())
        {
            throw new ArgumentException(
                $"Schedule with id {scheduleId} cannot be completed (status: {schedule.Status})");
        }

        Attraction attraction = await _attractionRepository.GetById(schedule.AttractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"Attraction with id {schedule.AttractionId} not found");
        }

        schedule.Status = MaintenanceStatus.Completed;
        _scheduleRepository.Update(schedule);

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