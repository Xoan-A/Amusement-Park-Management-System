namespace IDataAccess;

using Domain;

public interface IReportRepository
{
    Task<List<Report>> GetAllReports();
}