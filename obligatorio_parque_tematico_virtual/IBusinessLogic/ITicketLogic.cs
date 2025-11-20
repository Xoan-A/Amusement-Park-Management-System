using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface ITicketLogic
    {
        TicketResponse PurchaseTicket(PurchaseTicketRequest request);
        TicketResponse GetTicketById(Guid id);
        IEnumerable<TicketResponse> GetVisitorTickets(Guid visitorId);
        TicketResponse GetTicketByQRCode(Guid qrCode);
        bool ValidateTicket(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId, Guid attractionId);
    }
}