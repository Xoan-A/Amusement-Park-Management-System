using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Models.In;
using Models.Out;
using Domain;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketLogic _ticketLogic;

        public TicketController(ITicketLogic ticketLogic)
        {
            _ticketLogic = ticketLogic;
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketRequest request)
        {
            Ticket ticket = await _ticketLogic.PurchaseTicketAsync(request.VisitorId, request.VisitDate, request.TicketType, request.EventId);

            if (ticket == null)
            {
                return BadRequest("Unable to purchase ticket. Please check visitor ID and visit date.");
            }

            TicketResponse response = new TicketResponse
            {
                Id = ticket.Id,
                VisitorId = ticket.VisitorId,
                PurchaseDate = ticket.PurchaseDate,
                VisitDate = ticket.VisitDate,
                Type = ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            Ticket ticket = await _ticketLogic.GetTicketByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            TicketResponse response = new TicketResponse
            {
                Id = ticket.Id,
                VisitorId = ticket.VisitorId,
                PurchaseDate = ticket.PurchaseDate,
                VisitDate = ticket.VisitDate,
                Type = ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            };

            return Ok(response);
        }

        [HttpGet("visitor/{visitorId}")]
        public async Task<IActionResult> GetVisitorTickets(Guid visitorId)
        {
            IEnumerable<Ticket> tickets = await _ticketLogic.GetVisitorTicketsAsync(visitorId);

            List<TicketResponse> responses = tickets.Select(ticket => new TicketResponse
            {
                Id = ticket.Id,
                VisitorId = ticket.VisitorId,
                PurchaseDate = ticket.PurchaseDate,
                VisitDate = ticket.VisitDate,
                Type = ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            }).ToList();

            return Ok(responses);
        }
    }
}