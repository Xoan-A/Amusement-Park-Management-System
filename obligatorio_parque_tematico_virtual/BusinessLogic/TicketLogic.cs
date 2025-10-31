using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;

namespace BusinessLogic
{
    public class TicketLogic : ITicketLogic
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDateTimeLogic _dateTimeLogic;
        private readonly IEventRepository _eventRepository;
        private readonly int _eventDurationHours = 4;

        public TicketLogic(ITicketRepository ticketRepository, IUserRepository userRepository,
            IDateTimeLogic dateTimeLogic, IEventRepository eventRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _dateTimeLogic = dateTimeLogic;
            _eventRepository = eventRepository;
        }

        public async Task<TicketResponse> PurchaseTicketAsync(PurchaseTicketRequest request)
        {
            Guid visitorId = request.VisitorId;
            DateTime visitDate = request.VisitDate;
            TicketType ticketType = (TicketType)request.TicketType;
            Guid? eventId = request.EventId;

            User visitor = await _userRepository.GetById(visitorId);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {visitorId} not found");
            }

            DateTime currentDateTime = await _dateTimeLogic.GetCurrentDateTime();
            if (visitDate.Date < currentDateTime.Date)
            {
                throw new ArgumentException("Visit date cannot be in the past");
            }

            Ticket newTicket = new Ticket
            {
                VisitorId = visitorId,
                PurchaseDate = currentDateTime,
                VisitDate = visitDate,
                Type = ticketType,
                QRCode = Guid.NewGuid(),
                EventId = eventId
            };

            Ticket addedTicket = await _ticketRepository.AddAsync(newTicket);
            Ticket savedTicket = await _ticketRepository.GetByIdAsync(addedTicket.Id);

            return MapToTicketResponse(savedTicket);
        }

        public async Task<TicketResponse> GetTicketByIdAsync(Guid id)
        {
            Ticket ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket with ID {id} not found");
            }

            return MapToTicketResponse(ticket);
        }

        public async Task<IEnumerable<TicketResponse>> GetVisitorTicketsAsync(Guid visitorId)
        {
            IEnumerable<Ticket> tickets = await _ticketRepository.GetByVisitorIdAsync(visitorId);
            return tickets.Select(MapToTicketResponse);
        }

        public async Task<TicketResponse> GetTicketByQRCodeAsync(Guid qrCode)
        {
            Ticket ticket = await _ticketRepository.GetByQRCodeAsync(qrCode);
            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket with QR code {qrCode} not found");
            }

            return MapToTicketResponse(ticket);
        }

        public async Task<bool> ValidateTicketAsync(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId)
        {
            bool isValid = false;

            if (qr == null && nfc == null)
                return isValid;

            Ticket? ticket;
            if (qr != null)
            {
                ticket = await _ticketRepository.GetByQRCodeAsync(qr.Value);
                isValid = ticket != null && ticket.VisitDate.Date == enterDate.Date && ticket.EventId == eventId;
            }
            else
            {
                IEnumerable<Ticket> tickets = await _ticketRepository.GetByVisitorIdAsync(nfc!.Value);
                ticket = tickets.FirstOrDefault(t => t.VisitDate.Date == enterDate.Date && t.EventId == eventId);
                isValid = ticket != null;
            }

            if (isValid && ticket is { EventId: not null })
            {
                Event ticketEvent = await _eventRepository.GetById(ticket.EventId.Value);
                if (ticketEvent.Date.Date != enterDate.Date || ticketEvent.Date.Hour > enterDate.Hour ||
                    ticketEvent.Date.Hour + _eventDurationHours < enterDate.Hour)
                    isValid = false;
            }

            return isValid;
        }

        private TicketResponse MapToTicketResponse(Ticket ticket)
        {
            return new TicketResponse
            {
                Id = ticket.Id,
                VisitorId = ticket.VisitorId,
                VisitorName = ticket.Visitor?.Name,
                VisitorLastName = ticket.Visitor?.LastName,
                PurchaseDate = ticket.PurchaseDate,
                VisitDate = ticket.VisitDate,
                Type = (int)ticket.Type,
                QRCode = ticket.QRCode,
                EventId = ticket.EventId
            };
        }
    }
}