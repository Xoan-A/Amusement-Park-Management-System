namespace BusinessLogic
{
    public interface IDateSubject
    {
        void Attach(IDateObserver observer);
        void Detach(IDateObserver observer);
        void NotifyDateChange();
        DateTime GetPreviousDateTime();
        DateTime GetCurrentDateTime();
    }
}
