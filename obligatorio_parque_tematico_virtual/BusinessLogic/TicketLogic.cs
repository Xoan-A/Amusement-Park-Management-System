using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using IBusinessLogic;
using IDataAccess;

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

        public async Task<Ticket> PurchaseTicketAsync(Guid visitorId, DateTime visitDate, TicketType ticketType,
            Guid? eventId)
        {
            User visitor = await _userRepository.GetById(visitorId);
            if (visitor == null)
            {
                return null;
            }

            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
            if (visitDate.Date < currentDateTime.Date)
            {
                return null;
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

            return await _ticketRepository.AddAsync(newTicket);
        }

        public async Task<Ticket> GetTicketByIdAsync(Guid id)
        {
            return await _ticketRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Ticket>> GetVisitorTicketsAsync(Guid visitorId)
        {
            return await _ticketRepository.GetByVisitorIdAsync(visitorId);
        }

        public async Task<Ticket> GetTicketByQRCodeAsync(Guid qrCode)
        {
            return await _ticketRepository.GetByQRCodeAsync(qrCode);
        }

        public async Task<bool> ValidateTicketAsync(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId)
        {
            bool isValid = false;

            if (qr == null && nfc == null)
                return isValid;

            Ticket? ticket;
            if (qr != null)
            {
                ticket = await GetTicketByQRCodeAsync(qr.Value);
                isValid = ticket != null && ticket.VisitDate.Date == enterDate.Date && ticket.EventId == eventId;
            }
            else
            {
                IEnumerable<Ticket> tickets = await GetVisitorTicketsAsync(nfc!.Value);
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
    }
}