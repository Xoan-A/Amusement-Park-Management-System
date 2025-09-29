using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models.In;
using Models.Out;
using IBusinessLogic;
using BusinessLogic;

namespace TestApi
{
    [TestClass]
    public class TicketControllerTest
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton<IDateTimeLogic>(provider => DateTimeLogic.Instance);
                    });
                });
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _client.Dispose();
            _factory.Dispose();
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

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = await _client.PostAsync("/api/tickets", purchaseContent);

            Assert.AreEqual(HttpStatusCode.OK, purchaseResponse.StatusCode);

            String purchaseResponseBody = await purchaseResponse.Content.ReadAsStringAsync();
            TicketResponse ticketResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(ticketResult);
            Assert.AreEqual(registerResult.Id, ticketResult.VisitorId);
            Assert.AreEqual(TicketType.General, ticketResult.Type);
            Assert.AreNotEqual(Guid.Empty, ticketResult.QRCode);
        }

        [TestMethod]
        public async Task TestPurchaseTicket_VisitorNotFound()
        {
            PurchaseTicketRequest request = new PurchaseTicketRequest
            {
                VisitorId = Guid.NewGuid(),
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = TicketType.General
            };

            HttpContent content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await _client.PostAsync("/api/tickets", content);

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

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(-7),
                TicketType = TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = await _client.PostAsync("/api/tickets", purchaseContent);

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

            PurchaseTicketRequest purchaseRequest = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = TicketType.General
            };

            HttpContent purchaseContent = new StringContent(
                JsonSerializer.Serialize(purchaseRequest),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage purchaseResponse = await _client.PostAsync("/api/tickets", purchaseContent);
            purchaseResponse.EnsureSuccessStatusCode();

            String purchaseResponseBody = await purchaseResponse.Content.ReadAsStringAsync();
            TicketResponse purchaseResult = JsonSerializer.Deserialize<TicketResponse>(purchaseResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/tickets/{purchaseResult.Id}");

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
            HttpResponseMessage response = await _client.GetAsync("/api/tickets/99999");

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

            PurchaseTicketRequest purchaseRequest1 = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(7),
                TicketType = TicketType.General
            };

            HttpContent purchaseContent1 = new StringContent(
                JsonSerializer.Serialize(purchaseRequest1),
                Encoding.UTF8,
                "application/json"
            );

            await _client.PostAsync("/api/tickets", purchaseContent1);

            PurchaseTicketRequest purchaseRequest2 = new PurchaseTicketRequest
            {
                VisitorId = registerResult.Id,
                VisitDate = DateTime.Now.AddDays(14),
                TicketType = TicketType.EventSpecial,
                EventId = 5
            };

            HttpContent purchaseContent2 = new StringContent(
                JsonSerializer.Serialize(purchaseRequest2),
                Encoding.UTF8,
                "application/json"
            );

            await _client.PostAsync("/api/tickets", purchaseContent2);

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/tickets/visitor/{registerResult.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            String getResponseBody = await getResponse.Content.ReadAsStringAsync();
            List<TicketResponse> tickets = JsonSerializer.Deserialize<List<TicketResponse>>(getResponseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.IsNotNull(tickets);
            Assert.AreEqual(2, tickets.Count);
            Assert.IsTrue(tickets.All(t => t.VisitorId == registerResult.Id));
        }
    }
}