using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class DateTimeLogic : IDateTimeLogic
    {
        private readonly IDateTimeRepository _dateTimeRepository;

        public DateTimeLogic(IDateTimeRepository dateTimeRepository)
        {
            _dateTimeRepository = dateTimeRepository;
        }

        public async Task<DateTime> GetCurrentDateTime()
        {
            return await _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
        }

        public async Task SetDateTime(DateTime dateTime)
        {
            await _dateTimeRepository.SetConfiguredDateTime(dateTime);
        }

        public async Task SetDateTime(string dateTimeString)
        {
            var dateTime = DateTime.Parse(dateTimeString);
            await _dateTimeRepository.SetConfiguredDateTime(dateTime);
        }
    }
}