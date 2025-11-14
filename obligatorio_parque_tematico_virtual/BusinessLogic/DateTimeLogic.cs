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

            foreach (IDateObserver observer in observers)
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

        public void NotifyDateChange()
        {
            foreach (IDateObserver observer in _observers)
            {
                observer.DateUpdated(this);
            }
        }

        public DateTime GetPreviousDateTime()
        {
            return _previousDateTime;
        }

        public DateTime GetCurrentDateTime()
        {
            return _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
        }

        public void SetDateTime(DateTime dateTime)
        {
            _previousDateTime = _dateTimeRepository.GetConfiguredDateTime() ?? DateTime.Now;
            _dateTimeRepository.SetConfiguredDateTime(dateTime);
            _currentDateTime = dateTime;
            NotifyDateChange();
        }
    }
}