namespace IBusinessLogic
{
    public interface IDateTimeLogic
    {
        Task<DateTime> GetCurrentDateTime();
        Task SetDateTime(DateTime dateTime);
    }
}