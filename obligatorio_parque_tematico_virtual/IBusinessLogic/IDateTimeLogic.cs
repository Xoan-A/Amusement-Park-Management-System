using System;

namespace IBusinessLogic
{
    public interface IDateTimeLogic
    {
        Task<DateTime> GetCurrentDateTime();
        Task SetDateTime(DateTime dateTime);
        Task SetDateTime(string dateTimeString);
    }
}