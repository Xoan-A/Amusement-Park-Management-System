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

    public MaintenanceController(IMaintenanceLogic maintenanceLogic)
    {
        _maintenanceLogic = maintenanceLogic;
    }

    #region Schedule Endpoints

    [HttpPost("schedules")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateSchedule([FromBody] MaintenanceScheduleRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var scheduleId = await _maintenanceLogic.CreateSchedule(request, userId);

        return CreatedAtAction(nameof(GetScheduleById), new { id = scheduleId }, new { id = scheduleId, message = "Schedule created successfully" });
    }

    [HttpGet("schedules")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetAllSchedules()
    {
        var schedules = await _maintenanceLogic.GetAllSchedules();
        return Ok(schedules);
    }

    [HttpGet("schedules/{id}")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetScheduleById(Guid id)
    {
        var schedule = await _maintenanceLogic.GetScheduleById(id);
        return Ok(schedule);
    }

    [HttpGet("schedules/attraction/{attractionId}")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetSchedulesByAttraction(Guid attractionId)
    {
        var schedules = await _maintenanceLogic.GetSchedulesByAttraction(attractionId);
        return Ok(schedules);
    }

    [HttpGet("schedules/overdue")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetOverdueSchedules()
    {
        var schedules = await _maintenanceLogic.GetOverdueSchedules();
        return Ok(schedules);
    }

    [HttpGet("schedules/upcoming")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetUpcomingSchedules([FromQuery] int days = 7)
    {
        var schedules = await _maintenanceLogic.GetUpcomingSchedules(days);
        return Ok(schedules);
    }

    [HttpPut("schedules/{id}/status")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateScheduleStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        await _maintenanceLogic.UpdateScheduleStatus(id, request.Status);
        return Ok(new { message = "Schedule status updated successfully" });
    }

    [HttpDelete("schedules/{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        await _maintenanceLogic.DeleteSchedule(id);
        return Ok(new { message = "Schedule deleted successfully" });
    }

    #endregion

    #region Record Endpoints

    [HttpPost("records")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> RecordMaintenance([FromBody] MaintenanceRecordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var recordId = await _maintenanceLogic.RecordMaintenance(request, userId);

        return CreatedAtAction(nameof(GetRecordById), new { id = recordId }, new { id = recordId, message = "Maintenance record created successfully" });
    }

    [HttpGet("records")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetAllRecords()
    {
        var records = await _maintenanceLogic.GetAllRecords();
        return Ok(records);
    }

    [HttpGet("records/{id}")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetRecordById(Guid id)
    {
        var record = await _maintenanceLogic.GetRecordById(id);
        return Ok(record);
    }

    [HttpGet("records/attraction/{attractionId}")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetRecordsByAttraction(Guid attractionId)
    {
        var records = await _maintenanceLogic.GetRecordsByAttraction(attractionId);
        return Ok(records);
    }

    [HttpGet("records/operator/{operatorId}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetRecordsByOperator(Guid operatorId)
    {
        var records = await _maintenanceLogic.GetRecordsByOperator(operatorId);
        return Ok(records);
    }

    [HttpGet("records/unscheduled")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetUnscheduledMaintenance()
    {
        var records = await _maintenanceLogic.GetUnscheduledMaintenance();
        return Ok(records);
    }

    [HttpGet("records/history/{attractionId}")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> GetMaintenanceHistory(Guid attractionId, [FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
    {
        var records = await _maintenanceLogic.GetMaintenanceHistory(attractionId, dateFrom, dateTo);
        return Ok(records);
    }

    #endregion

    #region Business Operations

    [HttpPost("schedules/{scheduleId}/complete")]
    [Authorize(Roles = "Administrator,Operator")]
    public async Task<IActionResult> CompleteMaintenance(Guid scheduleId, [FromBody] MaintenanceRecordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var recordId = await _maintenanceLogic.CompleteMaintenance(scheduleId, request, userId);

        return Ok(new { recordId, message = "Maintenance completed and recorded successfully" });
    }

    #endregion
}
