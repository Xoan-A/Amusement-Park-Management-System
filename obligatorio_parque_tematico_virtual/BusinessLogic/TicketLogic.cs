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

        public TicketResponse PurchaseTicket(PurchaseTicketRequest request)
        {
            Guid visitorId = request.VisitorId;
            DateTime visitDate = request.VisitDate;
            TicketType ticketType = (TicketType)request.TicketType;
            Guid? eventId = request.EventId;

            if (!Enum.IsDefined(typeof(TicketType), ticketType))
            {
                throw new ArgumentException($"Invalid ticket type: {request.TicketType}");
            }

            if (ticketType == TicketType.EventSpecial)
            {
                if (eventId == null)
                {
                    throw new ArgumentException("EventSpecial ticket type requires an event ID");
                }

                Event ticketEvent = _eventRepository.GetById(eventId.Value);
                if (ticketEvent == null)
                {
                    throw new KeyNotFoundException($"Event with ID {eventId} not found");
                }

                if (ticketEvent.Date.Date != visitDate.Date)
                {
                    throw new ArgumentException(
                        $"Visit date must match the event date ({ticketEvent.Date.Date:yyyy-MM-dd})");
                }
            }

            User visitor = _userRepository.GetById(visitorId);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {visitorId} not found");
            }

            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
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

            Ticket addedTicket = _ticketRepository.Add(newTicket);
            Ticket savedTicket = _ticketRepository.GetById(addedTicket.Id);

            return MapToTicketResponse(savedTicket);
        }

        public TicketResponse GetTicketById(Guid id)
        {
            Ticket ticket = _ticketRepository.GetById(id);
            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket with ID {id} not found");
            }

            return MapToTicketResponse(ticket);
        }

        public IEnumerable<TicketResponse> GetVisitorTickets(Guid visitorId)
        {
            User visitor = _userRepository.GetById(visitorId);
            if (visitor == null)
            {
                throw new KeyNotFoundException($"Visitor with ID {visitorId} not found");
            }

            IEnumerable<Ticket> tickets = _ticketRepository.GetByVisitorId(visitorId);
            return tickets.Select(MapToTicketResponse);
        }

        public TicketResponse GetTicketByQRCode(Guid qrCode)
        {
            Ticket ticket = _ticketRepository.GetByQRCode(qrCode);
            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket with QR code {qrCode} not found");
            }

            return MapToTicketResponse(ticket);
        }

        public bool ValidateTicket(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId, Guid attractionId)
        {
            bool isValid = false;

            if (qr == null && nfc == null)
                return isValid;

            Ticket? ticket;
            if (qr != null)
            {
                ticket = _ticketRepository.GetByQRCode(qr.Value);
                isValid = ticket != null && ticket.VisitDate.Date == enterDate.Date && ticket.EventId == eventId;
            }
            else
            {
                IEnumerable<Ticket> tickets = _ticketRepository.GetByVisitorId(nfc!.Value);
                ticket = tickets.FirstOrDefault(t => t.VisitDate.Date == enterDate.Date && t.EventId == eventId);
                isValid = ticket != null;
            }

            if (isValid && ticket?.Type == TicketType.EventSpecial && ticket.EventId != null)
            {
                Event ticketEvent = _eventRepository.GetById(ticket.EventId.Value);
                if (ticketEvent.Date.Date != enterDate.Date || ticketEvent.Hour > enterDate.Hour ||
                    ticketEvent.Date.Hour + _eventDurationHours < enterDate.Hour)
                    isValid = false;

                if (isValid && !ticketEvent.Attractions.Any(ea => ea.AttractionId == attractionId))
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