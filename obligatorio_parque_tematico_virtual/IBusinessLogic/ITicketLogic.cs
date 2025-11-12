using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface ITicketLogic
    {
        Task<TicketResponse> PurchaseTicketAsync(PurchaseTicketRequest request);
        Task<TicketResponse> GetTicketByIdAsync(Guid id);
        Task<IEnumerable<TicketResponse>> GetVisitorTicketsAsync(Guid visitorId);
        Task<TicketResponse> GetTicketByQRCodeAsync(Guid qrCode);
        Task<bool> ValidateTicketAsync(Guid? qr, Guid? nfc, DateTime enterDate, Guid? eventId, Guid attractionId);
    }
}