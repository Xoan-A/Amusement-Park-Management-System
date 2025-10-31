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
        public async Task PurchaseTicket_WithNullVisitorNavigation_ReturnsNullNames()
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
            mockTicketLogic.Setup(t => t.PurchaseTicketAsync(It.IsAny<PurchaseTicketRequest>()))
                .ReturnsAsync(ticketWithNullVisitor);

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
                .ReturnsAsync(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(visitor);

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
            await client.PostAsync("/api/auth/register", registerContent);

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

            HttpResponseMessage loginResponse = await client.PostAsync("/api/auth/login", loginContent);
            string loginBody = await loginResponse.Content.ReadAsStringAsync();
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

            HttpResponseMessage response = await client.PostAsync("/api/tickets", content);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            string responseBody = await response.Content.ReadAsStringAsync();
            TicketResponse result = JsonSerializer.Deserialize<TicketResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNull(result.VisitorName);
            Assert.IsNull(result.VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }

        [TestMethod]
        public async Task GetTicketById_WithNullVisitorNavigation_ReturnsNullNames()
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
            mockTicketLogic.Setup(t => t.GetTicketByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(ticketWithNullVisitor);

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
                .ReturnsAsync(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(visitor);

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
            await client.PostAsync("/api/auth/register", registerContent);

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

            HttpResponseMessage loginResponse = await client.PostAsync("/api/auth/login", loginContent);
            string loginBody = await loginResponse.Content.ReadAsStringAsync();
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/tickets/{ticketWithNullVisitor.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseBody = await response.Content.ReadAsStringAsync();
            TicketResponse result = JsonSerializer.Deserialize<TicketResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNull(result.VisitorName);
            Assert.IsNull(result.VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }

        [TestMethod]
        public async Task GetTicketByQRCode_WithNullVisitorNavigation_ReturnsNullNames()
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
            mockTicketLogic.Setup(t => t.GetTicketByQRCodeAsync(qrCode))
                .ReturnsAsync(ticketWithNullVisitor);

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
                .ReturnsAsync(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(visitor);

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
            await client.PostAsync("/api/auth/register", registerContent);

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

            HttpResponseMessage loginResponse = await client.PostAsync("/api/auth/login", loginContent);
            string loginBody = await loginResponse.Content.ReadAsStringAsync();
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/tickets/qr/{qrCode}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseBody = await response.Content.ReadAsStringAsync();
            TicketResponse result = JsonSerializer.Deserialize<TicketResponse>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
            Assert.IsNull(result.VisitorName);
            Assert.IsNull(result.VisitorLastName);

            factory.Dispose();
            client.Dispose();
            connection.Close();
            connection.Dispose();
        }

        [TestMethod]
        public async Task GetVisitorTickets_WithNullVisitorNavigation_ReturnsNullNames()
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
            mockTicketLogic.Setup(t => t.GetVisitorTicketsAsync(visitorId))
                .ReturnsAsync(ticketsWithNullVisitor);

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
                .ReturnsAsync(visitor);

            mockAuthLogic.Setup(a => a.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(visitor);

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
            await client.PostAsync("/api/auth/register", registerContent);

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

            HttpResponseMessage loginResponse = await client.PostAsync("/api/auth/login", loginContent);
            string loginBody = await loginResponse.Content.ReadAsStringAsync();
            LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(loginBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/tickets/visitor/{visitorId}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string responseBody = await response.Content.ReadAsStringAsync();
            List<TicketResponse> result = JsonSerializer.Deserialize<List<TicketResponse>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(result);
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
