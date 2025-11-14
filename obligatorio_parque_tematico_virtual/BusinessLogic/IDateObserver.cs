namespace BusinessLogic
{
    public interface IDateObserver
    {
        void DateUpdated(IDateSubject subject);
    }
}
