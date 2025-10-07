using System;
using IBusinessLogic;
using IDataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic
{
    public class DateTimeLogic : IDateTimeLogic
    {
        private readonly IDateTimeRepository _dateTimeRepository;

        public DateTimeLogic(IDateTimeRepository dateTimeRepository)
        {
            _dateTimeRepository = dateTimeRepository;
        }

        public DateTime GetCurrentDateTime()
        {
            return _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
        }

        public void SetDateTime(DateTime dateTime)
        {
            _dateTimeRepository.SetConfiguredDateTime(dateTime);
        }

        public void SetDateTime(string dateTimeString)
        {
            var dateTime = DateTime.Parse(dateTimeString);
            _dateTimeRepository.SetConfiguredDateTime(dateTime);
        }
    }
}