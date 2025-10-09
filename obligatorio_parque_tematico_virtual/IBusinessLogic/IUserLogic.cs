using Domain;
using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface IUserLogic
    {
        Task<User> RegisterVisitor(RegisterVisitorRequest registerVisitorRequest);
        Task<UserResponse> CreateUser(CreateUserRequest request);
        Task RegisterEntry(Guid id, RegisterEntryRequest request);
        Task RegisterExit(Guid id, RegisterExitRequest request);
        Task<TopTenResponse> GetTopTenUsers();
        Task AddRoleToUser(Guid userId, string role);
        Task<User> GetUserById(Guid userId);
        Task<UserResponse> GetUserResponseById(Guid userId);
        Task<UserResponse> ModifyUser(Guid userId, string? actorSubClaim, ModifyUserRequest request);
    }
}