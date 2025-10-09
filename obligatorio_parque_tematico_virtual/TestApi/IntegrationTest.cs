using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Models.In;
using IBusinessLogic;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;
using IDataAccess;
using DataAccess.Repositories;

namespace ApiTests
{
    [TestClass]
    public class IntegrationTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private SqliteConnection _connection;

        [TestInitialize]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        ServiceDescriptor descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        ServiceDescriptor dateTimeDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IDateTimeLogic));
                        if (dateTimeDescriptor != null)
                        {
                            services.Remove(dateTimeDescriptor);
                        }

                        ServiceDescriptor dateTimeRepoDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IDateTimeRepository));
                        if (dateTimeRepoDescriptor != null)
                        {
                            services.Remove(dateTimeRepoDescriptor);
                        }

                        services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlite(_connection));

                        services.AddScoped<IDateTimeRepository, DateTimeRepository>();
                        services.AddScoped<IDateTimeLogic, DateTimeLogic>();
                    });
                });

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        [TestMethod]
        public async Task LoginEndpoint_RequiresNoAuthentication()
        {
            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test",
                LastName = "User",
                Email = "testlogin@test.com",
                Password = "password123",
                BirthDate = new System.DateTime(1990, 1, 1)
            };
            string registerJson = JsonSerializer.Serialize(registerRequest);
            StringContent registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/auth/register", registerContent);

            LoginRequest request = new LoginRequest
            {
                Email = "testlogin@test.com",
                Password = "password123"
            };

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/login", content);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task RegisterEndpoint_RequiresNoAuthentication()
        {
            RegisterVisitorRequest request = new RegisterVisitorRequest
            {
                Name = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            string json = JsonSerializer.Serialize(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/auth/register", content);

            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task DateTimeEndpoints_RequireNoAuthentication()
        {
            HttpResponseMessage getResponse = await _client.GetAsync("/api/datetime");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, getResponse.StatusCode);

            SetDateTimeRequest setRequest = new SetDateTimeRequest
            {
                DateTime = "2024-01-01T12:00:00"
            };

            string json = JsonSerializer.Serialize(setRequest);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage postResponse = await _client.PutAsync("/api/datetime", content);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, postResponse.StatusCode);
        }
    }
}