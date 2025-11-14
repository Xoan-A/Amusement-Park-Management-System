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
using Moq;
using Microsoft.EntityFrameworkCore;
using DataAccess.Context;
using Microsoft.Data.Sqlite;

namespace ApiTests
{
    [TestClass]
    public class BranchCoverageTest
    {
        [TestMethod]
        public void PurchaseTicket_WithNullVisitorNavigation_ReturnsNullNames()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            TicketResponse ticketWithNullVisitor = new TicketResponse
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                VisitorName = null,
                VisitorLastName = null,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = (int)TicketType.General,
                QRCode = Guid.NewGuid()
            };

            Mock<ITicketLogic> mockTicketLogic = new Mock<ITicketLogic>();
            mockTicketLogic.Setup(t => t.PurchaseTicket(It.IsAny<PurchaseTicketRequest>()))
            .Returns(ticketWithNullVisitor);

            Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
            Mock<IAuthLogic> mockAuthLogic = new Mock<IAuthLogic>();

            UserResponse visitor = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Visitor",
                Email = "test@test.com",
                UserRoles = new List<string> { Role.VISITOR }
            };

            mockUserLogic.Setup(u => u.RegisterVisitor(It.IsAny<RegisterVisitorRequest>()))
            .Returns(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(visitor);

            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbDescriptor != null) services.Remove(dbDescriptor);

                    ServiceDescriptor ticketDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ITicketLogic));
                    if (ticketDescriptor != null) services.Remove(ticketDescriptor);

                    ServiceDescriptor userDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IUserLogic));
                    if (userDescriptor != null) services.Remove(userDescriptor);

                    ServiceDescriptor authDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IAuthLogic));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
                    services.AddSingleton(mockTicketLogic.Object);
                    services.AddSingleton(mockUserLogic.Object);
                    services.AddSingleton(mockAuthLogic.Object);
                });
            });

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            HttpClient client = factory.CreateClient();

            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test",
                LastName = "Visitor",
                Email = "test@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            StringContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );
            _ = client.PostAsync("/api/auth/register", registerContent).Result;

            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test@test.com",
                Password = "password123"
            };

            StringContent loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage loginResponse = _ = client.PostAsync("/api/auth/login", loginContent).Result;
            string loginBody = loginResponse.Content.ReadAsStringAsync().Result;
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = Guid.NewGuid(),
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = (int)TicketType.General
            };

            StringContent content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = _ = client.PostAsync("/api/tickets", content).Result;

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            string responseBody = response.Content.ReadAsStringAsync().Result;
            TicketResponse result = JsonSerializer.Deserialize<TicketResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNull(result.VisitorName);
            Assert.IsNull(result.VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }

        [TestMethod]
        public void GetTicketById_WithNullVisitorNavigation_ReturnsNullNames()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            TicketResponse ticketWithNullVisitor = new TicketResponse
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                VisitorName = null,
                VisitorLastName = null,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = (int)TicketType.General,
                QRCode = Guid.NewGuid()
            };

            Mock<ITicketLogic> mockTicketLogic = new Mock<ITicketLogic>();
            mockTicketLogic.Setup(t => t.GetTicketById(It.IsAny<Guid>()))
            .Returns(ticketWithNullVisitor);

            Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
            Mock<IAuthLogic> mockAuthLogic = new Mock<IAuthLogic>();

            UserResponse visitor = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Visitor",
                Email = "test2@test.com",
                UserRoles = new List<string> { Role.VISITOR }
            };

            mockUserLogic.Setup(u => u.RegisterVisitor(It.IsAny<RegisterVisitorRequest>()))
            .Returns(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(visitor);

            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbDescriptor != null) services.Remove(dbDescriptor);

                    ServiceDescriptor ticketDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ITicketLogic));
                    if (ticketDescriptor != null) services.Remove(ticketDescriptor);

                    ServiceDescriptor userDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IUserLogic));
                    if (userDescriptor != null) services.Remove(userDescriptor);

                    ServiceDescriptor authDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IAuthLogic));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
                    services.AddSingleton(mockTicketLogic.Object);
                    services.AddSingleton(mockUserLogic.Object);
                    services.AddSingleton(mockAuthLogic.Object);
                });
            });

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            HttpClient client = factory.CreateClient();

            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test",
                LastName = "Visitor",
                Email = "test2@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            StringContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );
            _ = client.PostAsync("/api/auth/register", registerContent).Result;

            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test2@test.com",
                Password = "password123"
            };

            StringContent loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage loginResponse = _ = client.PostAsync("/api/auth/login", loginContent).Result;
            string loginBody = loginResponse.Content.ReadAsStringAsync().Result;
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            HttpResponseMessage response = _ = client.GetAsync($"/api/tickets/{ticketWithNullVisitor.Id}").Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseBody = response.Content.ReadAsStringAsync().Result;
            TicketResponse result = JsonSerializer.Deserialize<TicketResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNull(result.VisitorName);
            Assert.IsNull(result.VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }

        [TestMethod]
        public void GetTicketByQRCode_WithNullVisitorNavigation_ReturnsNullNames()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            Guid qrCode = Guid.NewGuid();
            TicketResponse ticketWithNullVisitor = new TicketResponse
            {
                Id = Guid.NewGuid(),
                VisitorId = Guid.NewGuid(),
                VisitorName = null,
                VisitorLastName = null,
                PurchaseDate = DateTime.Now,
                VisitDate = DateTime.Now.AddDays(7),
                Type = (int)TicketType.General,
                QRCode = qrCode
            };

            Mock<ITicketLogic> mockTicketLogic = new Mock<ITicketLogic>();
            mockTicketLogic.Setup(t => t.GetTicketByQRCode(qrCode))
            .Returns(ticketWithNullVisitor);

            Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
            Mock<IAuthLogic> mockAuthLogic = new Mock<IAuthLogic>();

            UserResponse visitor = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Visitor",
                Email = "test3@test.com",
                UserRoles = new List<string> { Role.VISITOR }
            };

            mockUserLogic.Setup(u => u.RegisterVisitor(It.IsAny<RegisterVisitorRequest>()))
            .Returns(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(visitor);

            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbDescriptor != null) services.Remove(dbDescriptor);

                    ServiceDescriptor ticketDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ITicketLogic));
                    if (ticketDescriptor != null) services.Remove(ticketDescriptor);

                    ServiceDescriptor userDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IUserLogic));
                    if (userDescriptor != null) services.Remove(userDescriptor);

                    ServiceDescriptor authDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IAuthLogic));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
                    services.AddSingleton(mockTicketLogic.Object);
                    services.AddSingleton(mockUserLogic.Object);
                    services.AddSingleton(mockAuthLogic.Object);
                });
            });

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            HttpClient client = factory.CreateClient();

            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test",
                LastName = "Visitor",
                Email = "test3@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            StringContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );
            _ = client.PostAsync("/api/auth/register", registerContent).Result;

            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test3@test.com",
                Password = "password123"
            };

            StringContent loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage loginResponse = _ = client.PostAsync("/api/auth/login", loginContent).Result;
            string loginBody = loginResponse.Content.ReadAsStringAsync().Result;
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            HttpResponseMessage response = _ = client.GetAsync($"/api/tickets/qr/{qrCode}").Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseBody = response.Content.ReadAsStringAsync().Result;
            TicketResponse result = JsonSerializer.Deserialize<TicketResponse>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNull(result.VisitorName);
            Assert.IsNull(result.VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }

        [TestMethod]
        public void GetVisitorTickets_WithNullVisitorNavigation_ReturnsNullNames()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            Guid visitorId = Guid.NewGuid();
            List<TicketResponse> ticketsWithNullVisitor = new List<TicketResponse>
            {
                new TicketResponse
                {
                    Id = Guid.NewGuid(),
                    VisitorId = visitorId,
                    VisitorName = null,
                    VisitorLastName = null,
                    PurchaseDate = DateTime.Now,
                    VisitDate = DateTime.Now.AddDays(7),
                    Type = (int)TicketType.General,
                    QRCode = Guid.NewGuid()
                }
            };

            Mock<ITicketLogic> mockTicketLogic = new Mock<ITicketLogic>();
            mockTicketLogic.Setup(t => t.GetVisitorTickets(visitorId))
            .Returns(ticketsWithNullVisitor);

            Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
            Mock<IAuthLogic> mockAuthLogic = new Mock<IAuthLogic>();

            UserResponse visitor = new UserResponse
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                LastName = "Visitor",
                Email = "test4@test.com",
                UserRoles = new List<string> { Role.VISITOR }
            };

            mockUserLogic.Setup(u => u.RegisterVisitor(It.IsAny<RegisterVisitorRequest>()))
            .Returns(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(visitor);

            WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    ServiceDescriptor dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbDescriptor != null) services.Remove(dbDescriptor);

                    ServiceDescriptor ticketDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ITicketLogic));
                    if (ticketDescriptor != null) services.Remove(ticketDescriptor);

                    ServiceDescriptor userDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IUserLogic));
                    if (userDescriptor != null) services.Remove(userDescriptor);

                    ServiceDescriptor authDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IAuthLogic));
                    if (authDescriptor != null) services.Remove(authDescriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
                    services.AddSingleton(mockTicketLogic.Object);
                    services.AddSingleton(mockUserLogic.Object);
                    services.AddSingleton(mockAuthLogic.Object);
                });
            });

            using (IServiceScope scope = factory.Services.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();
            }

            HttpClient client = factory.CreateClient();

            RegisterVisitorRequest registerRequest = new RegisterVisitorRequest
            {
                Name = "Test",
                LastName = "Visitor",
                Email = "test4@test.com",
                Password = "password123",
                BirthDate = new DateTime(1990, 1, 1)
            };

            StringContent registerContent = new StringContent(
                JsonSerializer.Serialize(registerRequest),
                Encoding.UTF8,
                "application/json"
            );
            _ = client.PostAsync("/api/auth/register", registerContent).Result;

            LoginRequest loginRequest = new LoginRequest
            {
                Email = "test4@test.com",
                Password = "password123"
            };

            StringContent loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage loginResponse = _ = client.PostAsync("/api/auth/login", loginContent).Result;
            string loginBody = loginResponse.Content.ReadAsStringAsync().Result;
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            HttpResponseMessage response = _ = client.GetAsync($"/api/tickets/visitor/{visitorId}").Result;

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseBody = response.Content.ReadAsStringAsync().Result;
            List<TicketResponse> result = JsonSerializer.Deserialize<List<TicketResponse>>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result[0].VisitorName);
            Assert.IsNull(result[0].VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }
    }
}