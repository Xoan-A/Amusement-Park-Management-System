using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IBusinessLogic;
using Models.In;
using Models.Out;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceLogic _maintenanceLogic;
    private readonly IClaimsLogic _claimsLogic;

    public MaintenanceController(IMaintenanceLogic maintenanceLogic, IClaimsLogic claimsLogic)
    {
        _maintenanceLogic = maintenanceLogic;
        _claimsLogic = claimsLogic;
    }

    #region Schedule Endpoints

    [HttpPost("schedules")]
    [Authorize(Roles = "Administrator")]
    public IActionResult CreateSchedule([FromBody] MaintenanceScheduleRequest request)
    {
        Guid scheduleId = _maintenanceLogic.CreateSchedule(request);

        return CreatedAtAction(nameof(GetScheduleById), new { id = scheduleId },
            new { id = scheduleId, message = "Schedule created successfully" });
    }

    [HttpGet("schedules")]
    [Authorize(Roles = "Administrator,Operator")]
    public IActionResult GetAllSchedules()
    {
        List<MaintenanceScheduleResponse> schedules = _maintenanceLogic.GetAllSchedules();
        return Ok(schedules);
    }

    [HttpGet("schedules/{id}")]
    [Authorize(Roles = "Administrator,Operator")]
    public IActionResult GetScheduleById(Guid id)
    {
        MaintenanceScheduleResponse schedule = _maintenanceLogic.GetScheduleById(id);
        return Ok(schedule);
    }

    [HttpGet("schedules/attraction/{attractionId}")]
    [Authorize(Roles = "Administrator,Operator")]
    public IActionResult GetSchedulesByAttraction(Guid attractionId)
    {
        List<MaintenanceScheduleResponse> schedules = _maintenanceLogic.GetSchedulesByAttraction(attractionId);
        return Ok(schedules);
    }

    [HttpGet("schedules/overdue")]
    [Authorize(Roles = "Administrator,Operator")]
    public IActionResult GetOverdueSchedules()
    {
        List<MaintenanceScheduleResponse> schedules = _maintenanceLogic.GetOverdueSchedules();
        return Ok(schedules);
    }

    [HttpGet("schedules/upcoming")]
    [Authorize(Roles = "Administrator,Operator")]
    public IActionResult GetUpcomingSchedules([FromQuery] int days = 7)
    {
        List<MaintenanceScheduleResponse> schedules = _maintenanceLogic.GetUpcomingSchedules(days);
        return Ok(schedules);
    }

    [HttpPut("schedules/{id}/status")]
    [Authorize(Roles = "Administrator")]
    public IActionResult UpdateScheduleStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        _maintenanceLogic.UpdateScheduleStatus(id, request.Status);
        return Ok(new { message = "Schedule status updated successfully" });
    }

    [HttpDelete("schedules/{id}")]
    [Authorize(Roles = "Administrator")]
    public IActionResult DeleteSchedule(Guid id)
    {
        _maintenanceLogic.DeleteSchedule(id);
        return Ok(new { message = "Schedule deleted successfully" });
    }

    #endregion

    #region Business Operations

    [HttpPost("schedules/{scheduleId}/complete")]
    [Authorize(Roles = "Administrator,Operator")]
    public IActionResult CompleteMaintenance(Guid scheduleId)
    {
        Guid userId = _claimsLogic.GetCurrentUserId(User);
        Guid completedScheduleId = _maintenanceLogic.CompleteMaintenance(scheduleId, userId);

        return Ok(new { scheduleId = completedScheduleId, message = "Maintenance completed successfully" });
    }

    #endregion
}