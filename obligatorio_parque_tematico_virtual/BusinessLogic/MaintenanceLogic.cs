using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class MaintenanceLogic : IMaintenanceLogic, IDateObserver
{
    private readonly IMaintenanceScheduleRepository _scheduleRepository;
    private readonly IMaintenanceRecordRepository _recordRepository;
    private readonly IAttractionRepository _attractionRepository;
    private readonly IAttractionLogic _attractionLogic;

    public MaintenanceLogic(
        IMaintenanceScheduleRepository scheduleRepository,
        IMaintenanceRecordRepository recordRepository,
        IAttractionRepository attractionRepository,
        IAttractionLogic attractionLogic)
    {
        _scheduleRepository = scheduleRepository;
        _recordRepository = recordRepository;
        _attractionRepository = attractionRepository;
        _attractionLogic = attractionLogic;
    }

    public async Task DateUpdated(IDateSubject subject)
    {
        DateTime currentDateTime = await subject.GetCurrentDateTime();

        List<MaintenanceSchedule> allSchedules = await _scheduleRepository.GetAllAsync();

        foreach (MaintenanceSchedule schedule in allSchedules)
        {
            bool wasUpdated = false;
            bool statusChangedToInProgress = false;

            if (schedule.Status == MaintenanceStatus.Pending && schedule.ScheduledDate <= currentDateTime)
            {
                schedule.Status = MaintenanceStatus.InProgress;
                statusChangedToInProgress = true;
                wasUpdated = true;
            }

            bool isOverdue = schedule.Status == MaintenanceStatus.InProgress &&
                             schedule.ScheduledDate.AddHours(schedule.EstimatedDuration) <= currentDateTime;

            if (schedule.IsOverdue != isOverdue)
            {
                schedule.IsOverdue = isOverdue;
                wasUpdated = true;
            }

            if (wasUpdated)
            {
                await _scheduleRepository.UpdateAsync(schedule);
            }

            if (statusChangedToInProgress)
            {
                string incidentMessage = $"Mantenimiento programado: {schedule.Description}";
                await _attractionLogic.AddIncident(schedule.AttractionId, incidentMessage);
            }
        }
    }

    #region Schedule Management

    public async Task<Guid> CreateSchedule(MaintenanceScheduleRequest request)
    {
        Attraction attraction = await _attractionRepository.GetById(request.AttractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"Attraction with id {request.AttractionId} not found");
        }

        MaintenanceSchedule schedule = new MaintenanceSchedule
        {
            Id = Guid.NewGuid(),
            AttractionId = request.AttractionId,
            ScheduledDate = request.ScheduledDate,
            Description = request.Description,
            EstimatedDuration = request.EstimatedDuration,
            Status = MaintenanceStatus.Pending,
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


        MaintenanceRecord record = new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            MaintenanceScheduleId = request.MaintenanceScheduleId,
            AttractionId = request.AttractionId,
            PerformedDate = request.PerformedDate,
            PerformedBy = performedBy,
            Description = request.Description,
            Notes = request.Notes,
            Duration = request.Duration,
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

        string incidentMessage = $"Mantenimiento programado: {schedule.Description}";
        await _attractionLogic.RemoveIncident(schedule.AttractionId, incidentMessage);

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
            Description = schedule.Description,
            EstimatedDuration = schedule.EstimatedDuration,
            Status = schedule.Status.ToString(),
            IsOverdue = schedule.IsOverdue
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
            Description = record.Description,
            Notes = record.Notes,
            Duration = record.Duration,
            CreatedAt = record.CreatedAt
        };
    }

    #endregion
}