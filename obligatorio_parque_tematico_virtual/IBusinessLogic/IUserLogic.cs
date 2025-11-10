using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface IUserLogic
    {
        Task<UserResponse> RegisterVisitor(RegisterVisitorRequest registerVisitorRequest);
        Task<UserResponse> CreateUser(CreateUserRequest request);
        Task RegisterEntry(Guid id, RegisterEntryRequest request);
        Task RegisterExit(Guid id, RegisterExitRequest request);
        Task<TopTenResponse> GetTopTenUsers();
        Task AddRoleToUser(Guid userId, string role);
        Task<UserResponse> GetUserResponseById(Guid userId);
        Task<UserResponse> ModifyUser(Guid userId, Guid userTokenId, ModifyUserRequest request);
        Task<UserResponse> ChangeMembershipLevel(Guid userId, int membershipLevel);
    }
}