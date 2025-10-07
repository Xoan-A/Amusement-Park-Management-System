using DataAccess.Context;
using IDataAccess;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Report>> GetAllReports()
    {
        return await _context.Reports.ToListAsync();
    }
}