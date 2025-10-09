using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Visitor")]
        public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketRequest request)
        {
            Ticket ticket = await _ticketLogic.PurchaseTicketAsync(request);

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
                Type = (int)ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            };

            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public async Task<IActionResult> GetTicketById(Guid id)
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
                Type = (int)ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            };

            return Ok(response);
        }

        [HttpGet("visitor/{visitorId}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public async Task<IActionResult> GetVisitorTickets(Guid visitorId)
        {
            IEnumerable<Ticket> tickets = await _ticketLogic.GetVisitorTicketsAsync(visitorId);

            List<TicketResponse> responses = tickets.Select(ticket => new TicketResponse
            {
                Id = ticket.Id,
                VisitorId = ticket.VisitorId,
                PurchaseDate = ticket.PurchaseDate,
                VisitDate = ticket.VisitDate,
                Type = (int)ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            }).ToList();

            return Ok(responses);
        }
    }
}