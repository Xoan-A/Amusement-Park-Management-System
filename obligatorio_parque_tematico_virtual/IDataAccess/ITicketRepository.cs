using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace IDataAccess
{
    public interface ITicketRepository
    {
        Task<Ticket> AddAsync(Ticket ticket);
        Task<Ticket> GetByIdAsync(Guid id);
        Task<IEnumerable<Ticket>> GetByVisitorIdAsync(Guid visitorId);
        Task<Ticket> GetByQRCodeAsync(Guid qrCode);
    }
}