using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<Ticket> AddAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Ticket>> GetByVisitorIdAsync(Guid visitorId)
        {
            return await _context.Tickets
                .Where(t => t.VisitorId == visitorId)
                .ToListAsync();
        }

        public async Task<Ticket> GetByQRCodeAsync(Guid qrCode)
        {
            return await _context.Tickets.FirstOrDefaultAsync(t => t.QRCode == qrCode);
        }

        public async Task<IEnumerable<Ticket>> GetByVisitDateAsync(DateTime visitDate)
        {
            return await _context.Tickets
                .Where(t => t.VisitDate.Date == visitDate.Date)
                .ToListAsync();
        }
    }
}