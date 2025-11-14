using DataAccess.Context;
using Domain;
using IDataAccess;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _context;

        public TicketRepository(AppDbContext context)
        {
            _context = context;
        }

        public Ticket Add(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
            return ticket;
        }

        public Ticket GetById(Guid id)
        {
            return _context.Tickets
            .Include(t => t.Visitor)
            .FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<Ticket> GetByVisitorId(Guid visitorId)
        {
            return _context.Tickets
            .Include(t => t.Visitor)
            .Where(t => t.VisitorId == visitorId)
            .ToList();
        }

        public Ticket GetByQRCode(Guid qrCode)
        {
            return _context.Tickets
            .Include(t => t.Visitor)
            .FirstOrDefault(t => t.QRCode == qrCode);
        }
    }
}