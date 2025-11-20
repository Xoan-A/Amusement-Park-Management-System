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
        public IActionResult PurchaseTicket([FromBody] PurchaseTicketRequest request)
        {
            TicketResponse response = _ticketLogic.PurchaseTicket(request);
            return CreatedAtAction(nameof(GetTicketById), new { id = response.Id }, response);
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public IActionResult GetTicketByQRCode(Guid qrCode)
        {
            TicketResponse response = _ticketLogic.GetTicketByQRCode(qrCode);
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public IActionResult GetTicketById(Guid id)
        {
            TicketResponse response = _ticketLogic.GetTicketById(id);
            return Ok(response);
        }

        [HttpGet("visitor/{visitorId}")]
        [Authorize(Roles = "Visitor, Operator, Administrator")]
        public IActionResult GetVisitorTickets(Guid visitorId)
        {
            IEnumerable<TicketResponse> responses = _ticketLogic.GetVisitorTickets(visitorId);

            return Ok(responses);
        }
    }
}