using AutoMapper;
using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;
using Domain.Exceptions;

namespace BusinessLogic;

public class UserManagementLogic : IUserManagementLogic
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordLogic _passwordLogic;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserValidationService _validationService;
    private readonly IParkEntryLogic _parkEntryLogic;
    private readonly IMapper _mapper;

    public UserManagementLogic(IUserRepository userRepository, IPasswordLogic passwordLogic,
        IRoleRepository roleRepository, IUserValidationService validationService,
        IParkEntryLogic parkEntryLogic, IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordLogic = passwordLogic;
        _roleRepository = roleRepository;
        _validationService = validationService;
        _parkEntryLogic = parkEntryLogic;
        _mapper = mapper;
    }

    public UserResponse RegisterVisitor(RegisterVisitorRequest request)
    {
        string name = request.Name;
        string lastName = request.LastName;
        string email = request.Email;
        string password = request.Password;
        DateTime birthDate = request.BirthDate;

        _validationService.ValidateRequiredFields(name, lastName, email, password);

        if (!_validationService.ValidateEmail(email))
            throw new ArgumentException("Invalid email format.");

        _validationService.ValidateBirthDate(birthDate);

        _validationService.ValidateEmailUniqueness(email);

        string hashedPassword = _passwordLogic.HashPassword(password);

        User visitor = new User
        {
            Name = name,
            LastName = lastName,
            Email = email,
            Password = hashedPassword,
            BirthDate = birthDate,
            MembershipLevel = MembershipLevel.Standard
        };

        Role? visitorRole = _roleRepository.GetByName(Role.Visitor);
        if (visitorRole != null)
        {
            visitor.UserRoles.Add(new UserRole
            { UserId = visitor.Id, RoleId = visitorRole.Id, Role = visitorRole });
        }

        User res = _userRepository.Create(visitor);

        return _mapper.Map<UserResponse>(res);
    }

    public UserResponse CreateUser(CreateUserRequest request)
    {
        string name = request.Name;
        string lastName = request.LastName;
        string email = request.Email;
        string password = request.Password;
        DateTime? birthDate = request.BirthDate;
        int? membershipLevel = request.MembershipLevel;
        List<string> roles = request.Roles;

        _validationService.ValidateRequiredFields(name, lastName, email, password);

        if (!_validationService.ValidateEmail(email))
            throw new ArgumentException("Invalid email format.");

        _validationService.ValidateEmailUniqueness(email);

        if (birthDate.HasValue)
        {
            _validationService.ValidateBirthDate(birthDate.Value);
        }

        string hashedPassword = _passwordLogic.HashPassword(password);

        User user = new User
        {
            Name = name,
            LastName = lastName,
            Email = email,
            Password = hashedPassword,
            BirthDate = birthDate
        };

        if (membershipLevel.HasValue)
        {
            _validationService.ValidateMembershipLevel(membershipLevel.Value);
            user.MembershipLevel = (MembershipLevel)membershipLevel.Value;
        }

        if (roles != null && roles.Count > 0)
        {
            foreach (string roleName in roles)
            {
                Role? role = _roleRepository.GetByName(roleName);
                if (role != null)
                {
                    user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });
                }
            }
        }

        User returnedUser = _userRepository.Create(user);
        return _mapper.Map<UserResponse>(returnedUser);
    }

    public UserResponse GetUserResponseById(Guid userId)
    {
        User user = _userRepository.GetByIdWithRoles(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return _mapper.Map<UserResponse>(user);
    }
    
    public void RegisterEntry(Guid attractionId, RegisterEntryRequest request)
    {
        _parkEntryLogic.RegisterEntry(attractionId, request);
    }

    public void RegisterExit(Guid attractionId, RegisterExitRequest request)
    {
        _parkEntryLogic.RegisterExit(attractionId, request);
    }

    public TopTenResponse GetTopTenUsers()
    {
        List<User> users = _userRepository.GetTopTen();
        return new TopTenResponse
        {
            TopTenUsers = _mapper.Map<List<UserResponse>>(users)
        };
    }

    public void AddRoleToUser(Guid userId, string roleName)
    {
        User user = _userRepository.GetByIdWithRoles(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        Role? role = _roleRepository.GetByName(roleName);
        if (role == null)
        {
            throw new ArgumentException("Role not found.");
        }

        if (user.UserRoles.Any(ur => ur.Role == role))
        {
            throw new ArgumentException("User already has that role.");
        }

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

        _userRepository.Update(user);
    }

    public UserResponse ModifyUser(Guid userId, Guid actorUserId, ModifyUserRequest request)
    {
        if (actorUserId != userId)
        {
            throw new ForbiddenException("You cannot modify another user");
        }

        User user = _userRepository.GetByIdWithRoles(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            user.Name = request.Name;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!_validationService.ValidateEmail(request.Email))
                throw new ArgumentException("Invalid email format.");

            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                bool unique = _userRepository.IsEmailUnique(request.Email);
                if (!unique)
                    throw new ArgumentException("Email must be unique");
                user.Email = request.Email;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            string hashedPassword = _passwordLogic.HashPassword(request.Password);
            user.Password = hashedPassword;
        }

        if (request.BirthDate.HasValue)
        {
            _validationService.ValidateBirthDate(request.BirthDate.Value);
            user.BirthDate = request.BirthDate.Value;
        }

        _userRepository.Update(user);

        return _mapper.Map<UserResponse>(user);
    }

    public UserResponse ChangeMembershipLevel(Guid userId, int membershipLevel)
    {
        if (!Enum.IsDefined(typeof(MembershipLevel), membershipLevel))
            throw new ArgumentException("Invalid membership level.");

        User user = _userRepository.GetByIdWithRoles(userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        user.MembershipLevel = (MembershipLevel)membershipLevel;
        _userRepository.Update(user);

        return _mapper.Map<UserResponse>(user);
    }

    public List<UserResponse> GetAllUsers()
    {
        List<User> users = _userRepository.GetAllUsers();
        return _mapper.Map<List<UserResponse>>(users);
    }
}