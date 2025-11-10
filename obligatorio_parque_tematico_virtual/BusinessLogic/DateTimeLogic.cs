using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class DateTimeLogic : IDateTimeLogic, IDateSubject
    {
        private readonly IDateTimeRepository _dateTimeRepository;
        private readonly List<IDateObserver> _observers = new List<IDateObserver>();
        private DateTime _previousDateTime;
        private DateTime _currentDateTime;

        public DateTimeLogic(IDateTimeRepository dateTimeRepository, IEnumerable<IDateObserver> observers)
        {
            _dateTimeRepository = dateTimeRepository;

            foreach (var observer in observers)
            {
                Attach(observer);
            }
        }

        public void Attach(IDateObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }

        public void Detach(IDateObserver observer)
        {
            _observers.Remove(observer);
        }

        public async Task NotifyDateChange()
        {
            foreach (var observer in _observers)
            {
                await observer.DateUpdated(this);
            }
        }

        public DateTime GetPreviousDateTime()
        {
            return _previousDateTime;
        }

        public async Task<DateTime> GetCurrentDateTime()
        {
            return await _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
        }

        public async Task SetDateTime(DateTime dateTime)
        {
            _previousDateTime = await _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
            await _dateTimeRepository.SetConfiguredDateTime(dateTime);
            _currentDateTime = dateTime;
            await NotifyDateChange();
        }
    }
}