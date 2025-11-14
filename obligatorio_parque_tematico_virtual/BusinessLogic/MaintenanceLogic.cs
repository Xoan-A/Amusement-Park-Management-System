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

    public void DateUpdated(IDateSubject subject)
    {
        DateTime currentDateTime = subject.GetCurrentDateTime();

        List<MaintenanceSchedule> allSchedules = _scheduleRepository.GetAll();

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
                _scheduleRepository.Update(schedule);
            }

            if (statusChangedToInProgress)
            {
                string incidentMessage = $"Mantenimiento programado: {schedule.Description}";
                _attractionLogic.AddIncident(schedule.AttractionId, incidentMessage);
            }
        }
    }

    #region Schedule Management

    public Guid CreateSchedule(MaintenanceScheduleRequest request)
    {
        Attraction attraction = _attractionRepository.GetById(request.AttractionId);
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

        _scheduleRepository.Create(schedule);
        return schedule.Id;
    }

    public MaintenanceScheduleResponse GetScheduleById(Guid id)
    {
        MaintenanceSchedule? schedule = _scheduleRepository.GetById(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {id} not found");
        }

        return MapToScheduleResponse(schedule);
    }

    public List<MaintenanceScheduleResponse> GetAllSchedules()
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetAll();
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public List<MaintenanceScheduleResponse> GetSchedulesByAttraction(Guid attractionId)
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetByAttractionId(attractionId);
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public List<MaintenanceScheduleResponse> GetOverdueSchedules()
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetOverdueSchedules();
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public List<MaintenanceScheduleResponse> GetUpcomingSchedules(int daysAhead)
    {
        List<MaintenanceSchedule> schedules = _scheduleRepository.GetUpcomingSchedules(daysAhead);
        return schedules.Select(MapToScheduleResponse).ToList();
    }

    public void UpdateScheduleStatus(Guid id, string status)
    {
        MaintenanceSchedule? schedule = _scheduleRepository.GetById(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {id} not found");
        }

        if (!Enum.TryParse<MaintenanceStatus>(status, out MaintenanceStatus maintenanceStatus) ||
            !Enum.IsDefined(typeof(MaintenanceStatus), maintenanceStatus))
        {
            throw new ArgumentException($"Invalid status: {status}");
        }

        schedule.Status = maintenanceStatus;
        if (schedule.Status == MaintenanceStatus.Completed)
        {
            schedule.IsOverdue = false;
        }

        _scheduleRepository.Update(schedule);
    }

    public void DeleteSchedule(Guid id)
    {
        _scheduleRepository.Delete(id);
    }

    #endregion

    #region Business Operations

    public Guid CompleteMaintenance(Guid scheduleId, Guid performedBy)
    {
        MaintenanceSchedule? schedule = _scheduleRepository.GetById(scheduleId);
        if (schedule == null)
        {
            throw new KeyNotFoundException($"Schedule with id {scheduleId} not found");
        }

        if (!schedule.CanComplete())
        {
            throw new ArgumentException(
                $"Schedule with id {scheduleId} cannot be completed (status: {schedule.Status})");
        }

        Attraction? attraction = _attractionRepository.GetById(schedule.AttractionId);
        if (attraction == null)
        {
            throw new KeyNotFoundException($"Attraction with id {schedule.AttractionId} not found");
        }

        string incidentMessage = $"Mantenimiento programado: {schedule.Description}";
        _attractionLogic.RemoveIncident(schedule.AttractionId, incidentMessage);

        schedule.Status = MaintenanceStatus.Completed;
        schedule.IsOverdue = false;
        _scheduleRepository.Update(schedule);

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