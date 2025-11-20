using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;

namespace IDataAccess
{
    public interface ITicketRepository
    {
        Ticket Add(Ticket ticket);
        Ticket GetById(Guid id);
        IEnumerable<Ticket> GetByVisitorId(Guid visitorId);
        Ticket GetByQRCode(Guid qrCode);
    }
}