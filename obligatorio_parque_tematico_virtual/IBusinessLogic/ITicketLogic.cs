using Domain;
using Models.In;

namespace IBusinessLogic
{
    public interface ITicketLogic
    {
        Task<Ticket> PurchaseTicketAsync(PurchaseTicketRequest request);
        Task<Ticket> GetTicketByIdAsync(Guid id);
        Task<IEnumerable<Ticket>> GetVisitorTicketsAsync(Guid visitorId);
        Task<Ticket> GetTicketByQRCodeAsync(Guid qrCode);
        Task<bool> ValidateTicketAsync(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId);
    }
}