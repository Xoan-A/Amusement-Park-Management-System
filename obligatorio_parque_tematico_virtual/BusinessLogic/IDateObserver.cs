namespace BusinessLogic
{
    public interface IDateObserver
    {
        Task DateUpdated(IDateSubject subject);
    }
}
