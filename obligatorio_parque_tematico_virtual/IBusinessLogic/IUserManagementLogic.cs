using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface IUserManagementLogic
    {
        UserResponse RegisterVisitor(RegisterVisitorRequest registerVisitorRequest);
        UserResponse CreateUser(CreateUserRequest request);
        void RegisterEntry(Guid id, RegisterEntryRequest request);
        void RegisterExit(Guid id, RegisterExitRequest request);
        TopTenResponse GetTopTenUsers();
        void AddRoleToUser(Guid userId, string role);
        UserResponse GetUserResponseById(Guid userId);
        UserResponse ModifyUser(Guid userId, Guid userTokenId, ModifyUserRequest request);
        UserResponse ChangeMembershipLevel(Guid userId, int membershipLevel);
    }
}