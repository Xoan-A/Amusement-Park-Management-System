using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace IBusinessLogic
{
    public interface ITicketLogic
    {
        Task<Ticket> PurchaseTicketAsync(Guid visitorId, DateTime visitDate, TicketType ticketType, Guid? eventId);
        Task<Ticket> GetTicketByIdAsync(Guid id);
        Task<IEnumerable<Ticket>> GetVisitorTicketsAsync(Guid visitorId);
        Task<Ticket> GetTicketByQRCodeAsync(Guid qrCode);
        Task<bool> ValidateTicketAsync(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId);
    }
}