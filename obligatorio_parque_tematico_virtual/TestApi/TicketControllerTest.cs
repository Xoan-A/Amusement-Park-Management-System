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

        private HttpClient CreateAuthenticatedClient(string email, string password)
        {
            LoginRequest loginRequest = new LoginRequest { Email = email, Password = password };
            HttpContent loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );
            HttpResponseMessage loginResponse = _ = _client.PostAsync("/api/auth/login", loginContent).Result;
            loginResponse.EnsureSuccessStatusCode();
            String loginResponseBody = loginResponse.Content.ReadAsStringAsync().Result;
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = _factory.CreateClient();
            authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);
            return authenticatedClient;
        }

        [TestMethod]
        public void TestPurchaseTicket_Success()
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

            HttpResponseMessage registerResponse = _ = _client.PostAsync("/api/auth/register", registerContent).Result;
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = registerResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = CreateAuthenticatedClient("testuser@test.com", "password123");

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

            HttpResponseMessage purchaseResponse = _ = authenticatedClient.PostAsync("/api/tickets", purchaseContent).Result;

            Assert.AreEqual(HttpStatusCode.Created, purchaseResponse.StatusCode);

            String purchaseResponseBody = purchaseResponse.Content.ReadAsStringAsync().Result;
            TicketResponse ticketResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(registerResult.Id, ticketResult.VisitorId);
            Assert.AreEqual((int)TicketType.General, ticketResult.Type);
            Assert.AreNotEqual(Guid.Empty, ticketResult.QRCode);
            Assert.AreEqual("Test", ticketResult.VisitorName);
        }

        [TestMethod]
        public void TestPurchaseTicket_VisitorNotFound()
        {
            _ = _client.PostAsync("/api/auth/register", new StringContent(
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
            )).Result;
            HttpClient authenticatedClient = CreateAuthenticatedClient("authuser1@test.com", "password123");

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

            HttpResponseMessage response = _ = authenticatedClient.PostAsync("/api/tickets", content).Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestPurchaseTicket_PastVisitDate()
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

            HttpResponseMessage registerResponse = _ = _client.PostAsync("/api/auth/register", registerContent).Result;
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = registerResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = CreateAuthenticatedClient("testuser2@test.com", "password123");

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

            HttpResponseMessage purchaseResponse = _ = authenticatedClient.PostAsync("/api/tickets", purchaseContent).Result;

            Assert.AreEqual(HttpStatusCode.BadRequest, purchaseResponse.StatusCode);
        }

        [TestMethod]
        public void TestGetTicketById_Success()
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

            HttpResponseMessage registerResponse = _ = _client.PostAsync("/api/auth/register", registerContent).Result;
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = registerResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = CreateAuthenticatedClient("testuser3@test.com", "password123");

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

            HttpResponseMessage purchaseResponse = _ = authenticatedClient.PostAsync("/api/tickets", purchaseContent).Result;
            purchaseResponse.EnsureSuccessStatusCode();

            String purchaseResponseBody = purchaseResponse.Content.ReadAsStringAsync().Result;
            TicketResponse purchaseResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpResponseMessage getResponse = _ = authenticatedClient.GetAsync($"/api/tickets/{purchaseResult.Id}").Result;

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = getResponse.Content.ReadAsStringAsync().Result;
            TicketResponse getResult = JsonSerializer.Deserialize<TicketResponse>(getResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(purchaseResult.Id, getResult.Id);
            Assert.AreEqual(purchaseResult.QRCode, getResult.QRCode);
            Assert.AreEqual("Test3", getResult.VisitorName);
        }

        [TestMethod]
        public void TestGetTicketById_NotFound()
        {
            _ = _client.PostAsync("/api/auth/register", new StringContent(
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
            )).Result;
            HttpClient authenticatedClient = CreateAuthenticatedClient("authuser2@test.com", "password123");

            HttpResponseMessage response = authenticatedClient.GetAsync($"/api/tickets/{Guid.NewGuid()}").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestGetVisitorTickets_Success()
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

            HttpResponseMessage registerResponse = _ = _client.PostAsync("/api/auth/register", registerContent).Result;
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = registerResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = CreateAuthenticatedClient("testuser4@test.com", "password123");

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

            _ = authenticatedClient.PostAsync("/api/tickets", purchaseContent1).Result;

            PurchaseTicketRequest purchaseRequest2 = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(14),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent2 = new StringContent(
                JsonSerializer.Serialize(purchaseRequest2),
                Encoding.UTF8,
                "application/json"
            );

            _ = authenticatedClient.PostAsync("/api/tickets", purchaseContent2).Result;

            HttpResponseMessage getResponse =
            _ = authenticatedClient.GetAsync($"/api/tickets/visitor/{registerResult.Id}").Result;

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = getResponse.Content.ReadAsStringAsync().Result;
            List<TicketResponse> tickets = JsonSerializer.Deserialize<List<TicketResponse>>(getResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(2, tickets.Count);
            Assert.IsTrue(tickets.All(t => t.VisitorId == registerResult.Id));
            Assert.IsTrue(tickets.All(t => t.VisitorName == "Test4"));
            Assert.IsTrue(tickets.All(t => t.VisitorLastName == "User4"));
        }

        [TestMethod]
        public void TestPurchaseTicket_Unauthorized()
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

            HttpResponseMessage response = _ = _client.PostAsync("/api/tickets", content).Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestGetTicketById_Unauthorized()
        {
            HttpResponseMessage response = _ = _client.GetAsync("/api/tickets/1").Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestGetVisitorTickets_Unauthorized()
        {
            HttpResponseMessage response = _client.GetAsync($"/api/tickets/visitor/{Guid.NewGuid()}").Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestGetTicketByQRCode_Success()
        {
            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "QRTest",
                LastName = "User",
                Email = "qrtest@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage registerResponse = _ = _client.PostAsync("/api/auth/register", registerContent).Result;
            registerResponse.EnsureSuccessStatusCode();

            String registerResponseBody = registerResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse registerResult = JsonSerializer.Deserialize<RegisterResponse>(registerResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient authenticatedClient = CreateAuthenticatedClient("qrtest@test.com", "password123");

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

            HttpResponseMessage purchaseResponse = _ = authenticatedClient.PostAsync("/api/tickets", purchaseContent).Result;
            purchaseResponse.EnsureSuccessStatusCode();

            String purchaseResponseBody = purchaseResponse.Content.ReadAsStringAsync().Result;
            TicketResponse purchaseResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpResponseMessage getResponse =
            _ = authenticatedClient.GetAsync($"/api/tickets/qr/{purchaseResult.QRCode}").Result;

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = getResponse.Content.ReadAsStringAsync().Result;
            TicketResponse getResult = JsonSerializer.Deserialize<TicketResponse>(getResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(purchaseResult.Id, getResult.Id);
            Assert.AreEqual(purchaseResult.QRCode, getResult.QRCode);
            Assert.AreEqual(purchaseResult.VisitorId, getResult.VisitorId);
            Assert.AreEqual("QRTest", getResult.VisitorName);
        }

        [TestMethod]
        public void TestGetTicketByQRCode_NotFound()
        {
            _ = _client.PostAsync("/api/auth/register", new StringContent(
                JsonSerializer.Serialize(new RegisterVisitorRequest
                {
                    Name = "QRAuth",
                    LastName = "User",
                    Email = "qrauth@test.com",
                    Password = "password123",
                    BirthDate = new DateTime(1990, 1, 1)
                }),
                Encoding.UTF8,
                "application/json"
            )).Result;
            HttpClient authenticatedClient = CreateAuthenticatedClient("qrauth@test.com", "password123");

            HttpResponseMessage response = authenticatedClient.GetAsync($"/api/tickets/qr/{Guid.NewGuid()}").Result;

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestGetTicketByQRCode_Unauthorized()
        {
            HttpResponseMessage response = _client.GetAsync($"/api/tickets/qr/{Guid.NewGuid()}").Result;

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public void TestGetTicketByQRCode_OperatorCanAccess()
        {
            RegisterVisitorRequest visitorRequest = new RegisterVisitorRequest
            {
                Name = "QRVisitor",
                LastName = "User",
                Email = "qrvisitor@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent visitorContent = new StringContent(
                JsonSerializer.Serialize(visitorRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage visitorResponse = _ = _client.PostAsync("/api/auth/register", visitorContent).Result;
            visitorResponse.EnsureSuccessStatusCode();

            String visitorResponseBody = visitorResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse visitorResult = JsonSerializer.Deserialize<RegisterResponse>(visitorResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient visitorClient = CreateAuthenticatedClient("qrvisitor@test.com", "password123");

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = visitorResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = _ = visitorClient.PostAsync("/api/tickets", purchaseContent).Result;
            purchaseResponse.EnsureSuccessStatusCode();

            String purchaseResponseBody = purchaseResponse.Content.ReadAsStringAsync().Result;
            TicketResponse purchaseResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                IPasswordLogic passwordLogic = scope.ServiceProvider.GetRequiredService<IPasswordLogic>();

                Role operatorRole = context.Roles.FirstOrDefault(r => r.Name == Role.OPERATOR);
                if (operatorRole == null)
                {
                    operatorRole = new Role { Name = Role.OPERATOR };
                    context.Roles.Add(operatorRole);
                    context.SaveChanges();
                }

                User operatorUser = new User
                {
                    Name = "Test",
                    LastName = "Operator",
                    Email = "qroperator@test.com",
                    Password = passwordLogic.HashPassword("password123")
                };
                operatorUser.UserRoles = new List<UserRole>
                {
                    new UserRole { Role = operatorRole }
                };
                context.Users.Add(operatorUser);
                context.SaveChanges();
            }

            HttpClient operatorClient = CreateAuthenticatedClient("qroperator@test.com", "password123");

            HttpResponseMessage getResponse = _ = operatorClient.GetAsync($"/api/tickets/qr/{purchaseResult.QRCode}").Result;

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = getResponse.Content.ReadAsStringAsync().Result;
            TicketResponse getResult = JsonSerializer.Deserialize<TicketResponse>(getResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(purchaseResult.QRCode, getResult.QRCode);
        }

        [TestMethod]
        public void TestGetTicketByQRCode_AdministratorCanAccess()
        {
            RegisterVisitorRequest visitorRequest = new RegisterVisitorRequest
            {
                Name = "QRVisitor2",
                LastName = "User",
                Email = "qrvisitor2@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            HttpContent visitorContent = new StringContent(
                JsonSerializer.Serialize(visitorRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage visitorResponse = _ = _client.PostAsync("/api/auth/register", visitorContent).Result;
            visitorResponse.EnsureSuccessStatusCode();

            String visitorResponseBody = visitorResponse.Content.ReadAsStringAsync().Result;
            RegisterResponse visitorResult = JsonSerializer.Deserialize<RegisterResponse>(visitorResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpClient visitorClient = CreateAuthenticatedClient("qrvisitor2@test.com", "password123");

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = visitorResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = _ = visitorClient.PostAsync("/api/tickets", purchaseContent).Result;
            purchaseResponse.EnsureSuccessStatusCode();

            String purchaseResponseBody = purchaseResponse.Content.ReadAsStringAsync().Result;
            TicketResponse purchaseResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            using (IServiceScope scope = _factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                IPasswordLogic passwordLogic = scope.ServiceProvider.GetRequiredService<IPasswordLogic>();

                Role adminRole = context.Roles.FirstOrDefault(r => r.Name == Role.ADMINISTRATOR);
                if (adminRole == null)
                {
                    adminRole = new Role { Name = Role.ADMINISTRATOR };
                    context.Roles.Add(adminRole);
                    context.SaveChanges();
                }

                User adminUser = new User
                {
                    Name = "Test",
                    LastName = "Admin",
                    Email = "qradmin@test.com",
                    Password = passwordLogic.HashPassword("password123")
                };
                adminUser.UserRoles = new List<UserRole>
                {
                    new UserRole { Role = adminRole }
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            HttpClient adminClient = CreateAuthenticatedClient("qradmin@test.com", "password123");

            HttpResponseMessage getResponse = _ = adminClient.GetAsync($"/api/tickets/qr/{purchaseResult.QRCode}").Result;

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = getResponse.Content.ReadAsStringAsync().Result;
            TicketResponse getResult = JsonSerializer.Deserialize<TicketResponse>(getResponseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(purchaseResult.QRCode, getResult.QRCode);
        }
    }
}