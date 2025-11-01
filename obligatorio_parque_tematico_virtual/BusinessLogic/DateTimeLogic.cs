using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class DateTimeLogic : IDateTimeLogic
    {
        private readonly IDateTimeRepository _dateTimeRepository;
        private readonly IUserRepository _userRepository;

        public DateTimeLogic(IDateTimeRepository dateTimeRepository, IUserRepository userRepository)
        {
            _dateTimeRepository = dateTimeRepository;
            _userRepository = userRepository;
        }

        public async Task<DateTime> GetCurrentDateTime()
        {
            return await _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
        }

        public async Task SetDateTime(DateTime dateTime)
        {
            await _dateTimeRepository.SetConfiguredDateTime(dateTime);
            await _userRepository.ResetScores();
        }

        public async Task SetDateTime(string dateTimeString)
        {
            DateTime dateTime = DateTime.Parse(dateTimeString);
            await _dateTimeRepository.SetConfiguredDateTime(dateTime);
            await _userRepository.ResetScores();
        }
    }
}