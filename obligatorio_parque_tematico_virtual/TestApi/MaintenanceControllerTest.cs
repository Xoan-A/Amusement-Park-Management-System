using System.IdentityModel.Tokens.Jwt;
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
using Microsoft.IdentityModel.Tokens;

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

        Microsoft.Extensions.Options.IOptions<Models.JwtSettings> jwtSettings =
        Microsoft.Extensions.Options.Options.Create(new Models.JwtSettings
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
        Guid scheduleId = Guid.NewGuid();
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Monthly safety inspection",
            EstimatedDuration = 120
        };

        _mockMaintenanceLogic.Setup(m => m.CreateSchedule(It.IsAny<MaintenanceScheduleRequest>()))
        .ReturnsAsync(scheduleId);

        StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _adminClient.PostAsync("/api/maintenance/schedules", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseBody.Contains(scheduleId.ToString()));
    }

    [TestMethod]
    public async Task CreateSchedule_AsOperator_ReturnsForbidden()
    {
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Monthly safety inspection",
            EstimatedDuration = 120
        };

        StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _operatorClient.PostAsync("/api/maintenance/schedules", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task GetAllSchedules_AsAdministrator_ReturnsOk()
    {
        List<MaintenanceScheduleResponse> schedules = new List<MaintenanceScheduleResponse>
        {
            new MaintenanceScheduleResponse
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                AttractionName = "Roller Coaster",
                ScheduledDate = DateTime.Now.AddDays(7),
                Description = "Monthly inspection",
                EstimatedDuration = 120,
                Status = "Pending",
                IsOverdue = false
            }
        };

        _mockMaintenanceLogic.Setup(m => m.GetAllSchedules()).ReturnsAsync(schedules);

        HttpResponseMessage response = await _adminClient.GetAsync("/api/maintenance/schedules");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(responseBody.Contains("Roller Coaster"));
    }

    [TestMethod]
    public async Task GetScheduleById_ExistingSchedule_ReturnsOk()
    {
        Guid scheduleId = Guid.NewGuid();
        MaintenanceScheduleResponse schedule = new MaintenanceScheduleResponse
        {
            Id = scheduleId,
            AttractionId = Guid.NewGuid(),
            AttractionName = "Roller Coaster",
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Monthly inspection",
            EstimatedDuration = 120,
            Status = "Pending",
            IsOverdue = false
        };

        _mockMaintenanceLogic.Setup(m => m.GetScheduleById(scheduleId)).ReturnsAsync(schedule);

        HttpResponseMessage response = await _adminClient.GetAsync($"/api/maintenance/schedules/{scheduleId}");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetOverdueSchedules_ReturnsOk()
    {
        List<MaintenanceScheduleResponse> schedules = new List<MaintenanceScheduleResponse>
        {
            new MaintenanceScheduleResponse
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                AttractionName = "Roller Coaster",
                ScheduledDate = DateTime.Now.AddDays(-1),
                Description = "Overdue inspection",
                EstimatedDuration = 60,
                Status = "Pending",
                IsOverdue = true
            }
        };

        _mockMaintenanceLogic.Setup(m => m.GetOverdueSchedules()).ReturnsAsync(schedules);

        HttpResponseMessage response = await _adminClient.GetAsync("/api/maintenance/schedules/overdue");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetUpcomingSchedules_ReturnsOk()
    {
        List<MaintenanceScheduleResponse> schedules = new List<MaintenanceScheduleResponse>();
        _mockMaintenanceLogic.Setup(m => m.GetUpcomingSchedules(7)).ReturnsAsync(schedules);

        HttpResponseMessage response = await _adminClient.GetAsync("/api/maintenance/schedules/upcoming?days=7");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetSchedulesByAttraction_AsAdministrator_ReturnsOk()
    {
        Guid attractionId = Guid.NewGuid();
        List<MaintenanceScheduleResponse> schedules = new List<MaintenanceScheduleResponse>
        {
            new MaintenanceScheduleResponse
            {
                Id = Guid.NewGuid(),
                AttractionId = attractionId,
                AttractionName = "Roller Coaster",
                ScheduledDate = DateTime.Now.AddDays(3),
                Description = "Routine inspection",
                EstimatedDuration = 90,
                Status = "Pending",
                IsOverdue = false
            }
        };
        _mockMaintenanceLogic.Setup(m => m.GetSchedulesByAttraction(attractionId)).ReturnsAsync(schedules);

        HttpResponseMessage response =
        await _adminClient.GetAsync($"/api/maintenance/schedules/attraction/{attractionId}");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateScheduleStatus_AsAdministrator_ReturnsOk()
    {
        Guid scheduleId = Guid.NewGuid();
        object request = new { status = "Completed" };
        StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        _mockMaintenanceLogic.Setup(m => m.UpdateScheduleStatus(scheduleId, "Completed"))
        .Returns(Task.CompletedTask);

        HttpResponseMessage response =
        await _adminClient.PutAsync($"/api/maintenance/schedules/{scheduleId}/status", content);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSchedule_AsAdministrator_ReturnsOk()
    {
        Guid scheduleId = Guid.NewGuid();
        _mockMaintenanceLogic.Setup(m => m.DeleteSchedule(scheduleId)).Returns(Task.CompletedTask);

        HttpResponseMessage response = await _adminClient.DeleteAsync($"/api/maintenance/schedules/{scheduleId}");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSchedule_AsOperator_ReturnsForbidden()
    {
        Guid scheduleId = Guid.NewGuid();

        HttpResponseMessage response = await _operatorClient.DeleteAsync($"/api/maintenance/schedules/{scheduleId}");

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Business Operations

    [TestMethod]
    public async Task CompleteMaintenance_AsOperator_ReturnsOk()
    {
        Guid scheduleId = Guid.NewGuid();
        Guid recordId = Guid.NewGuid();

        _mockMaintenanceLogic.Setup(m =>
        m.CompleteMaintenance(scheduleId, It.IsAny<Guid>()))
        .ReturnsAsync(recordId);

        HttpResponseMessage response =
        await _operatorClient.PostAsync($"/api/maintenance/schedules/{scheduleId}/complete", null);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task CompleteMaintenance_AsVisitor_ReturnsForbidden()
    {
        Guid scheduleId = Guid.NewGuid();

        HttpResponseMessage response =
        await _visitorClient.PostAsync($"/api/maintenance/schedules/{scheduleId}/complete", null);

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Error Path Tests

    [TestMethod]
    public async Task CreateSchedule_WithInvalidData_ReturnsBadRequest()
    {
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.Now.AddDays(7),
            Description = "Test"
        };

        _mockMaintenanceLogic.Setup(m => m.CreateSchedule(It.IsAny<MaintenanceScheduleRequest>()))
        .ThrowsAsync(new ArgumentException("Invalid data"));

        StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _adminClient.PostAsync("/api/maintenance/schedules", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task GetScheduleById_NonExistentSchedule_ReturnsNotFound()
    {
        Guid scheduleId = Guid.NewGuid();
        _mockMaintenanceLogic.Setup(m => m.GetScheduleById(scheduleId))
        .ThrowsAsync(new KeyNotFoundException("Schedule not found"));

        HttpResponseMessage response = await _adminClient.GetAsync($"/api/maintenance/schedules/{scheduleId}");

        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task CompleteMaintenance_NonExistentSchedule_ReturnsNotFound()
    {
        Guid scheduleId = Guid.NewGuid();

        _mockMaintenanceLogic.Setup(m =>
        m.CompleteMaintenance(scheduleId, It.IsAny<Guid>()))
        .ThrowsAsync(new KeyNotFoundException("Schedule not found"));

        HttpResponseMessage response =
        await _operatorClient.PostAsync($"/api/maintenance/schedules/{scheduleId}/complete", null);

        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task CreateSchedule_WithMissingNameIdentifierClaim_UsesEmptyGuid()
    {
        MaintenanceScheduleRequest request = new MaintenanceScheduleRequest
        {
            AttractionId = Guid.NewGuid(),
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            Description = "Test"
        };

        _mockMaintenanceLogic.Setup(m => m.CreateSchedule(
            It.IsAny<MaintenanceScheduleRequest>()))
        .ReturnsAsync(Guid.NewGuid());

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890");
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Administrator")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        String tokenString = tokenHandler.WriteToken(token);

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        StringContent content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/maintenance/schedules", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [TestMethod]
    public async Task CompleteMaintenance_WithMissingNameIdentifierClaim_UsesEmptyGuid()
    {
        Guid scheduleId = Guid.NewGuid();

        _mockMaintenanceLogic.Setup(m =>
        m.CompleteMaintenance(scheduleId, It.IsAny<Guid>()))
        .ReturnsAsync(Guid.NewGuid());

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForJWTTokenGeneration1234567890");
        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Operator")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "ParqueTematico",
            Audience = "ParqueTematico",
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        String tokenString = tokenHandler.WriteToken(token);

        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        HttpResponseMessage response =
        await client.PostAsync($"/api/maintenance/schedules/{scheduleId}/complete", null);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}