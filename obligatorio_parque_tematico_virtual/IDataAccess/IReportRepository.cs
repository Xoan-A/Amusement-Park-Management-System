namespace IDataAccess;

using Domain;

public interface IReportRepository
{
    List<Report> GetAllReports();
}