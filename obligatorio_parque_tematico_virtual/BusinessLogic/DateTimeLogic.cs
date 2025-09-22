using System;
using IBusinessLogic;

namespace BusinessLogic
{
    public class DateTimeLogic : IDateTimeLogic
    {
        private static DateTimeLogic _instance;
        private static readonly object _lock = new object();
        private DateTime? _configuredDateTime;

        private DateTimeLogic() { }

        public static DateTimeLogic Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DateTimeLogic();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        public DateTime GetCurrentDateTime()
        {
            return _configuredDateTime ?? DateTime.Now;
        }

        public void SetDateTime(DateTime dateTime)
        {
            _configuredDateTime = dateTime;
        }

        public void SetDateTime(string dateTimeString)
        {
            _configuredDateTime = DateTime.Parse(dateTimeString);
        }
    }
}