using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Models.In;
using Models.Out;
using IBusinessLogic;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using IDataAccess;
using Microsoft.Data.Sqlite;
using DataAccess.Repositories;

namespace TestApi
{
    [TestClass]
    public class TicketControllerTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private SqliteConnection _connection;

        [TestInitialize]
        public void TestInitialize()
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

                        ServiceDescriptor? dateTimeDescriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(IDateTimeLogic));
                        if (dateTimeDescriptor != null)
                        {
                            services.Remove(dateTimeDescriptor);
                        }

                        ServiceDescriptor? dateTimeRepoDescriptor = services.SingleOrDefault(
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
        public void TestCleanup()
        {
            _client.Dispose();
            _factory.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        private async Task<HttpClient> CreateAuthenticatedClient(string email, string password)
        {
            LoginRequest loginRequest = new LoginRequest { Email = email, Password = password };
            HttpContent loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );
            HttpResponseMessage loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();
            String loginResponseBody = await loginResponse.Content.ReadAsStringAsync();
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = _factory.CreateClient();
            authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);
            return authenticatedClient;
        }

        [TestMethod]
        public async Task TestPurchaseTicket_Success()
        {
            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test",
                LastName = "User",
                Email = "testuser@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = await registerResponse.Content.ReadAsStringAsync();
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = await CreateAuthenticatedClient("testuser@test.com", "password123");

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = await authenticatedClient.PostAsync("/api/tickets", purchaseContent);

            Assert.AreEqual(HttpStatusCode.Created, purchaseResponse.StatusCode);

            String purchaseResponseBody = await purchaseResponse.Content.ReadAsStringAsync();
            TicketResponse ticketResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(ticketResult);
            Assert.AreEqual(registerResult.Id, ticketResult.VisitorId);
            Assert.AreEqual((int)TicketType.General, ticketResult.Type);
            Assert.AreNotEqual(Guid.Empty, ticketResult.QRCode);
        }

        [TestMethod]
        public async Task TestPurchaseTicket_VisitorNotFound()
        {
            await _client.PostAsync("/api/auth/register", new StringContent(
                JsonSerializer.Serialize(new RegisterVisitorRequest
                {
                    Name = "Auth",
                    LastName = "User",
                    Email = "authuser1@test.com",
                    Password = "password123",
                    BirthDate = new DateTime(1990, 1, 1)
                }),
                Encoding.UTF8,
                "application/json"
            ));
            HttpClient authenticatedClient = await CreateAuthenticatedClient("authuser1@test.com", "password123");

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = Guid.NewGuid(),
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await authenticatedClient.PostAsync("/api/tickets", content);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task TestPurchaseTicket_PastVisitDate()
        {
            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test2",
                LastName = "User2",
                Email = "testuser2@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = await registerResponse.Content.ReadAsStringAsync();
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = await CreateAuthenticatedClient("testuser2@test.com", "password123");

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(-7),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = await authenticatedClient.PostAsync("/api/tickets", purchaseContent);

            Assert.AreEqual(HttpStatusCode.BadRequest, purchaseResponse.StatusCode);
        }

        [TestMethod]
        public async Task TestGetTicketById_Success()
        {
            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test3",
                LastName = "User3",
                Email = "testuser3@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = await registerResponse.Content.ReadAsStringAsync();
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = await CreateAuthenticatedClient("testuser3@test.com", "password123");

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = await authenticatedClient.PostAsync("/api/tickets", purchaseContent);
            purchaseResponse.EnsureSuccessStatusCode();

            String purchaseResponseBody = await purchaseResponse.Content.ReadAsStringAsync();
            TicketResponse purchaseResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpResponseMessage getResponse = await authenticatedClient.GetAsync($"/api/tickets/{purchaseResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = await getResponse.Content.ReadAsStringAsync();
            TicketResponse getResult = JsonSerializer.Deserialize<TicketResponse>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(getResult);
            Assert.AreEqual(purchaseResult.Id, getResult.Id);
            Assert.AreEqual(purchaseResult.QRCode, getResult.QRCode);
        }

        [TestMethod]
        public async Task TestGetTicketById_NotFound()
        {
            await _client.PostAsync("/api/auth/register", new StringContent(
                JsonSerializer.Serialize(new RegisterVisitorRequest
                {
                    Name = "Auth",
                    LastName = "User",
                    Email = "authuser2@test.com",
                    Password = "password123",
                    BirthDate = new DateTime(1990, 1, 1)
                }),
                Encoding.UTF8,
                "application/json"
            ));
            HttpClient authenticatedClient = await CreateAuthenticatedClient("authuser2@test.com", "password123");

            HttpResponseMessage response = await authenticatedClient.GetAsync($"/api/tickets/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestGetVisitorTickets_Success()
        {
            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test4",
                LastName = "User4",
                Email = "testuser4@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = await registerResponse.Content.ReadAsStringAsync();
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = await CreateAuthenticatedClient("testuser4@test.com", "password123");

            PurchaseTicketRequest purchaseRequest1 = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent1 = new StringContent(
                JsonSerializer.Serialize(purchaseRequest1),
                Encoding.UTF8,
                "application/json"
            );

            await authenticatedClient.PostAsync("/api/tickets", purchaseContent1);

            PurchaseTicketRequest purchaseRequest2 = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(14),
                TicketType = (int)TicketType.EventSpecial,
                EventId = Guid.NewGuid()
            };

            HttpContent purchaseContent2 = new StringContent(
                JsonSerializer.Serialize(purchaseRequest2),
                Encoding.UTF8,
                "application/json"
            );

            await authenticatedClient.PostAsync("/api/tickets", purchaseContent2);

            HttpResponseMessage getResponse = await authenticatedClient.GetAsync($"/api/tickets/visitor/{registerResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = await getResponse.Content.ReadAsStringAsync();
            List<TicketResponse> tickets = JsonSerializer.Deserialize<List<TicketResponse>>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(tickets);
            Assert.AreEqual(2, tickets.Count);
            Assert.IsTrue(tickets.All(t => t.VisitorId == registerResult.Id));
        }

        [TestMethod]
        public async Task TestPurchaseTicket_Unauthorized()
        {
            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = Guid.NewGuid(),
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _client.PostAsync("/api/tickets", content);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task TestGetTicketById_Unauthorized()
        {
            HttpResponseMessage response = await _client.GetAsync("/api/tickets/1");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task TestGetVisitorTickets_Unauthorized()
        {
            HttpResponseMessage response = await _client.GetAsync($"/api/tickets/visitor/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}