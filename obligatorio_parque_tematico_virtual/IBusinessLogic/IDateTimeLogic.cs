using System;

namespace IBusinessLogic
{
    public interface IDateTimeLogic
    {
        DateTime GetCurrentDateTime();
        void SetDateTime(DateTime dateTime);
        void SetDateTime(string dateTimeString);
    }
}