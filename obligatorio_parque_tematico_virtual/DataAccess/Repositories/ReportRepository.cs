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

    public List<Report> GetAllReports()
    {
        return _context.Reports
        .Include(r => r.Attraction)
        .Include(r => r.VisitorReport).ToList();
    }
}