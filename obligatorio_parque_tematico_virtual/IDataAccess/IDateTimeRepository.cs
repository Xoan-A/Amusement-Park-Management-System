namespace IDataAccess;

public interface IDateTimeRepository
{
    Task<DateTime?> GetConfiguredDateTime();
    Task SetConfiguredDateTime(DateTime dateTime);
}