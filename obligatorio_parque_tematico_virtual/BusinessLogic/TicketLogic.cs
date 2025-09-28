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

        public TicketLogic(ITicketRepository ticketRepository, IUserRepository userRepository, IDateTimeLogic dateTimeLogic)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _dateTimeLogic = dateTimeLogic;
        }

        public async Task<Ticket> PurchaseTicketAsync(Guid visitorId, DateTime visitDate, TicketType ticketType, int? eventId)
        {
            User visitor = _userRepository.GetById(visitorId);
            if (visitor == null || visitor is not Visitor)
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

        public async Task<Ticket> GetTicketByIdAsync(int id)
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
    }
}