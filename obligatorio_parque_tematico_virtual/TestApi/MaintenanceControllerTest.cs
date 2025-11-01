using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text;
using System.Text.Json;
using Domain;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;

namespace ApiTests;

[TestClass]
public class MaintenanceControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _adminClient = null!;
    private HttpClient _operatorClient = null!;
    private HttpClient _visitorClient = null!;
    private Mock<IMaintenanceLogic> _mockMaintenanceLogic = null!;
    private SqliteConnection _connection = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockMaintenanceLogic = new Mock<IMaintenanceLogic>();

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(_connection));

                services.AddSingleton(_mockMaintenanceLogic.Object);
            });
        });

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings = Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
        {
            SecretKey = "MySecretKeyForJWTTokenGeneration1234567890",
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            ExpirationHours = 1
        });
        BusinessLogic.TokenLogic tokenService = new BusinessLogic.TokenLogic(jwtSettings);

        UserResponse adminUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            UserRoles = new List<string> { Role.ADMINISTRATOR }
        };
        string adminToken = tokenService.GenerateToken(adminUser);
        _adminClient = _factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        UserResponse operatorUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Operator",
            LastName = "User",
            Email = "operator@example.com",
            UserRoles = new List<string> { Role.OPERATOR }
        };
        string operatorToken = tokenService.GenerateToken(operatorUser);
        _operatorClient = _factory.CreateClient();
        _operatorClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", operatorToken);

        UserResponse visitorUser = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = "Visitor",
            LastName = "User",
            Email = "visitor@example.com",
            UserRoles = new List<string> { Role.VISITOR }
        };
        string visitorToken = tokenService.GenerateToken(visitorUser);
        _visitorClient = _factory.CreateClient();
        _visitorClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", visitorToken);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _adminClient?.Dispose();
        _operatorClient?.Dispose();
        _visitorClient?.Dispose();
        _factory?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }

    #region Schedule Endpoints

    [TestMethod]
    public async Task CreateSchedule_AsAdministrator_ReturnsCreated()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly safety inspection"
        };

        _mockMaintenanceLogic.Setup(m => m.CreateSchedule(It.IsAny<MaintenanceScheduleRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(scheduleId);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _adminClient.PostAsync("/api/maintenance/schedules", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseBody.Contains(scheduleId.ToString()));
    }

    [TestMethod]
    public async Task CreateSchedule_AsOperator_ReturnsForbidden()
    {
        // Arrange
        var request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly safety inspection"
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _operatorClient.PostAsync("/api/maintenance/schedules", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAllSchedules_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var schedules = new List<MaintenanceScheduleResponse>
        {
            new MaintenanceScheduleResponse
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                AttractionName = "Roller Coaster",
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = "Inspection",
                Description = "Monthly inspection",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                IsOverdue = false
            }
        };

        _mockMaintenanceLogic.Setup(m => m.GetAllSchedules()).ReturnsAsync(schedules);

        // Act
        var response = await _adminClient.GetAsync("/api/maintenance/schedules");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseBody.Contains("Roller Coaster"));
    }

    [TestMethod]
    public async Task GetScheduleById_ExistingSchedule_ReturnsOk()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var schedule = new MaintenanceScheduleResponse
        {
            Id = scheduleId,
            AttractionId = Guid.NewGuid(),
            AttractionName = "Roller Coaster",
            ScheduledDate = DateTime.Now.AddDays(7),
            MaintenanceType = "Inspection",
            Description = "Monthly inspection",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            IsOverdue = false
        };

        _mockMaintenanceLogic.Setup(m => m.GetScheduleById(scheduleId)).ReturnsAsync(schedule);

        // Act
        var response = await _adminClient.GetAsync($"/api/maintenance/schedules/{scheduleId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetOverdueSchedules_ReturnsOk()
    {
        // Arrange
        var schedules = new List<MaintenanceScheduleResponse>
        {
            new MaintenanceScheduleResponse
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                AttractionName = "Roller Coaster",
                ScheduledDate = DateTime.Now.AddDays(-1),
                MaintenanceType = "Inspection",
                Description = "Overdue inspection",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                IsOverdue = true
            }
        };

        _mockMaintenanceLogic.Setup(m => m.GetOverdueSchedules()).ReturnsAsync(schedules);

        // Act
        var response = await _adminClient.GetAsync("/api/maintenance/schedules/overdue");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetUpcomingSchedules_ReturnsOk()
    {
        // Arrange
        var schedules = new List<MaintenanceScheduleResponse>();
        _mockMaintenanceLogic.Setup(m => m.GetUpcomingSchedules(7)).ReturnsAsync(schedules);

        // Act
        var response = await _adminClient.GetAsync("/api/maintenance/schedules/upcoming?days=7");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var request = new { status = "Completed" };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        _mockMaintenanceLogic.Setup(m => m.UpdateScheduleStatus(scheduleId, "Completed"))
            .Returns(Task.CompletedTask);

        // Act
        var response = await _adminClient.PutAsync($"/api/maintenance/schedules/{scheduleId}/status", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSchedule_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        _mockMaintenanceLogic.Setup(m => m.DeleteSchedule(scheduleId)).Returns(Task.CompletedTask);

        // Act
        var response = await _adminClient.DeleteAsync($"/api/maintenance/schedules/{scheduleId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSchedule_AsOperator_ReturnsForbidden()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();

        // Act
        var response = await _operatorClient.DeleteAsync($"/api/maintenance/schedules/{scheduleId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Record Endpoints

    [TestMethod]
    public async Task RecordMaintenance_AsOperator_ReturnsCreated()
    {
        // Arrange
        var recordId = Guid.NewGuid();
        var request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Completed safety inspection",
            Duration = TimeSpan.FromHours(2)
        };

        _mockMaintenanceLogic.Setup(m => m.RecordMaintenance(It.IsAny<MaintenanceRecordRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(recordId);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _operatorClient.PostAsync("/api/maintenance/records", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task RecordMaintenance_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync("/api/maintenance/records", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAllRecords_AsAdministrator_ReturnsOk()
    {
        // Arrange
        var records = new List<MaintenanceRecordResponse>
        {
            new MaintenanceRecordResponse
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                AttractionName = "Roller Coaster",
                PerformedDate = DateTime.Now,
                PerformedBy = Guid.NewGuid(),
                PerformedByName = "John Operator",
                MaintenanceType = "Inspection",
                Description = "Completed inspection",
                Duration = TimeSpan.FromHours(2),
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockMaintenanceLogic.Setup(m => m.GetAllRecords()).ReturnsAsync(records);

        // Act
        var response = await _adminClient.GetAsync("/api/maintenance/records");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetRecordById_ExistingRecord_ReturnsOk()
    {
        // Arrange
        var recordId = Guid.NewGuid();
        var record = new MaintenanceRecordResponse
        {
            Id = recordId,
            AttractionId = Guid.NewGuid(),
            AttractionName = "Roller Coaster",
            PerformedDate = DateTime.Now,
            PerformedBy = Guid.NewGuid(),
            PerformedByName = "John Operator",
            MaintenanceType = "Inspection",
            Description = "Completed inspection",
            Duration = TimeSpan.FromHours(2),
            CreatedAt = DateTime.UtcNow
        };

        _mockMaintenanceLogic.Setup(m => m.GetRecordById(recordId)).ReturnsAsync(record);

        // Act
        var response = await _adminClient.GetAsync($"/api/maintenance/records/{recordId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetRecordsByAttraction_ReturnsOk()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var records = new List<MaintenanceRecordResponse>();
        _mockMaintenanceLogic.Setup(m => m.GetRecordsByAttraction(attractionId)).ReturnsAsync(records);

        // Act
        var response = await _adminClient.GetAsync($"/api/maintenance/records/attraction/{attractionId}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetUnscheduledMaintenance_ReturnsOk()
    {
        // Arrange
        var records = new List<MaintenanceRecordResponse>();
        _mockMaintenanceLogic.Setup(m => m.GetUnscheduledMaintenance()).ReturnsAsync(records);

        // Act
        var response = await _adminClient.GetAsync("/api/maintenance/records/unscheduled");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetMaintenanceHistory_ReturnsOk()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var dateFrom = DateTime.Now.AddDays(-30);
        var dateTo = DateTime.Now;
        var records = new List<MaintenanceRecordResponse>();

        _mockMaintenanceLogic.Setup(m => m.GetMaintenanceHistory(attractionId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(records);

        // Act
        var response = await _adminClient.GetAsync($"/api/maintenance/records/history/{attractionId}?dateFrom={dateFrom:O}&dateTo={dateTo:O}");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    #endregion

    #region Business Operations

    [TestMethod]
    public async Task CompleteMaintenance_AsOperator_ReturnsOk()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var recordId = Guid.NewGuid();
        var request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Completed scheduled maintenance",
            Duration = TimeSpan.FromHours(2)
        };

        _mockMaintenanceLogic.Setup(m => m.CompleteMaintenance(scheduleId, It.IsAny<MaintenanceRecordRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(recordId);

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _operatorClient.PostAsync($"/api/maintenance/schedules/{scheduleId}/complete", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task CompleteMaintenance_AsVisitor_ReturnsForbidden()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var request = new MaintenanceRecordRequest
        {
            AttractionId = Guid.NewGuid(),
            PerformedDate = DateTime.Now,
            MaintenanceType = "Inspection",
            Description = "Test",
            Duration = TimeSpan.FromHours(1)
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _visitorClient.PostAsync($"/api/maintenance/schedules/{scheduleId}/complete", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion
}
