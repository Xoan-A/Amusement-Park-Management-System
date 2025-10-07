using System;
using IBusinessLogic;
using IDataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic
{
    public class DateTimeLogic : IDateTimeLogic
    {
        private static DateTimeLogic _instance;
        private static readonly object _lock = new object();
        private DateTime? _configuredDateTime;
        private readonly IServiceProvider _serviceProvider;

        private DateTimeLogic(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static DateTimeLogic GetInstance(IServiceProvider serviceProvider)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DateTimeLogic(serviceProvider);
                    }
                }
            }
            return _instance;
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
            if (_configuredDateTime != null && _configuredDateTime.Value.Date < dateTime.Date)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    userRepository.ResetScores().Wait();
                }
            }

            _configuredDateTime = dateTime;
        }

        public void SetDateTime(string dateTimeString)
        {
            var dateTime = DateTime.Parse(dateTimeString);
            if (_configuredDateTime != null && _configuredDateTime.Value.Date < dateTime.Date)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                    userRepository.ResetScores().Wait();
                }
            }
            _configuredDateTime = dateTime;
        }
    }
}