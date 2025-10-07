namespace IDataAccess;

public interface IDateTimeRepository
{
    DateTime? GetConfiguredDateTime();
    void SetConfiguredDateTime(DateTime dateTime);
}