using Models.In;

namespace IBusinessLogic
{
    public interface IParkEntryLogic
    {
        void RegisterEntry(Guid attractionId, RegisterEntryRequest request);
        void RegisterExit(Guid attractionId, RegisterExitRequest request);
    }
}
