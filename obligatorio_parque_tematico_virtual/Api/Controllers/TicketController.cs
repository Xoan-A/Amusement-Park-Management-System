using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IBusinessLogic;
using Models.In;
using Models.Out;

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
            TicketResponse response = await _ticketLogic.PurchaseTicketAsync(request);
            return CreatedAtAction(nameof(GetTicketById), new { id = response.Id }, response);
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public async Task<IActionResult> GetTicketByQRCode(Guid qrCode)
        {
            TicketResponse response = await _ticketLogic.GetTicketByQRCodeAsync(qrCode);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public async Task<IActionResult> GetTicketById(Guid id)
        {
            TicketResponse response = await _ticketLogic.GetTicketByIdAsync(id);
            return Ok(response);
        }

        [HttpGet("visitor/{visitorId}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public async Task<IActionResult> GetVisitorTickets(Guid visitorId)
        {
            IEnumerable<TicketResponse> responses = await _ticketLogic.GetVisitorTicketsAsync(visitorId);

            return Ok(responses);
        }
    }
}