namespace BusinessLogic
{
    public interface IDateSubject
    {
        void Attach(IDateObserver observer);
        void Detach(IDateObserver observer);
        Task NotifyDateChange();
        DateTime GetPreviousDateTime();
        Task<DateTime> GetCurrentDateTime();
    }
}
