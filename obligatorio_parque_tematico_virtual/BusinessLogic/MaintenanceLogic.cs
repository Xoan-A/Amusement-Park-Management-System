using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic;

public class MaintenanceLogic : IMaintenanceLogic, IDateObserver
{
    private readonly IMaintenanceScheduleRepository _scheduleRepository;
    private readonly IAttractionRepository _attractionRepository;
    private readonly IAttractionLogic _attractionLogic;

    public MaintenanceLogic(
        IMaintenanceScheduleRepository scheduleRepository,
        IAttractionRepository attractionRepository,
        IAttractionLogic attractionLogic)
    {
        _scheduleRepository = scheduleRepository;
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
        if (schedule.Status == MaintenanceStatus.Completed)
        {
            schedule.IsOverdue = false;
        }
        await _scheduleRepository.UpdateAsync(schedule);
    }

    public async Task DeleteSchedule(Guid id)
    {
        await _scheduleRepository.DeleteAsync(id);
    }

    #endregion

    #region Business Operations

    public async Task<Guid> CompleteMaintenance(Guid scheduleId, Guid performedBy)
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
        schedule.IsOverdue = false;
        await _scheduleRepository.UpdateAsync(schedule);

        return scheduleId;
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

    #endregion
}