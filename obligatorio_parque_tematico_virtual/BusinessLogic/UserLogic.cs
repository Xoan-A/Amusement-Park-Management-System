using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.In;
using Models.Out;
using Domain.Exceptions;

namespace BusinessLogic
{
    public class UserLogic : IUserLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordLogic _passwordLogic;
        private readonly IAttractionRepository _attractionRepository;
        private readonly ITicketLogic _ticketLogic;
        private readonly IRoleRepository _roleRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IDailyScoreLogic _dailyScoreLogic;
        private readonly IDateTimeLogic _dateTimeLogic;

        public UserLogic(IUserRepository userRepository, IPasswordLogic passwordLogic,
            IAttractionRepository attractionRepository, ITicketLogic ticketLogic, IRoleRepository roleRepository,
            IEventRepository eventRepository, IDailyScoreLogic dailyScoreLogic, IDateTimeLogic dateTimeLogic)
        {
            _userRepository = userRepository;
            _passwordLogic = passwordLogic;
            _attractionRepository = attractionRepository;
            _ticketLogic = ticketLogic;
            _roleRepository = roleRepository;
            _eventRepository = eventRepository;
            _dailyScoreLogic = dailyScoreLogic;
            _dateTimeLogic = dateTimeLogic;
        }

        public async Task<UserResponse> RegisterVisitor(RegisterVisitorRequest request)
        {
            string name = request.Name;
            string lastName = request.LastName;
            string email = request.Email;
            string password = request.Password;
            DateTime birthDate = request.BirthDate;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Name, last name, email, and password must be provided.");

            DateTime currentDateTime = await _dateTimeLogic.GetCurrentDateTime();

            if (birthDate >= currentDateTime)
                throw new ArgumentException("Birth date cannot be after today.");

            if (!await _userRepository.IsEmailUnique(email))
                throw new ArgumentException("Email is already in use.");

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

            Role? visitorRole = await _roleRepository.GetByNameAsync(Role.VISITOR);
            if (visitorRole != null)
            {
                visitor.UserRoles.Add(new UserRole { UserId = visitor.Id, RoleId = visitorRole.Id, Role = visitorRole });
            }

            User res = await _userRepository.Create(visitor);

            return new UserResponse
            {
                Id = res.Id,
                Name = res.Name,
                LastName = res.LastName,
                Email = res.Email,
                BirthDate = res.BirthDate,
                MembershipLevel = (int?)res.MembershipLevel,
                UserRoles = res.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Score = res.Score
            };
        }

        public async Task<UserResponse> CreateUser(CreateUserRequest request)
        {
            string name = request.Name;
            string lastName = request.LastName;
            string email = request.Email;
            string password = request.Password;
            DateTime? birthDate = request.BirthDate;
            string? membershipLevel = request.MembershipLevel;
            List<string> roles = request.Roles;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Name, last name, email, and password must be provided.");
            }

            if (!await _userRepository.IsEmailUnique(email))
            {
                throw new ArgumentException("Email is already in use.");
            }

            if (birthDate.HasValue)
            {
                DateTime currentDateTime = await _dateTimeLogic.GetCurrentDateTime();
                if (birthDate.Value >= currentDateTime)
                    throw new ArgumentException("Birth date cannot be after today.");
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

            if (!string.IsNullOrEmpty(membershipLevel))
            {
                if (Enum.TryParse<MembershipLevel>(membershipLevel, true, out MembershipLevel parsedLevel))
                {
                    user.MembershipLevel = parsedLevel;
                }
                else
                {
                    throw new ArgumentException("Invalid membership level.");
                }
            }

            if (roles != null && roles.Count > 0)
            {
                foreach (string roleName in roles)
                {
                    Role? role = await _roleRepository.GetByNameAsync(roleName);
                    if (role != null)
                    {
                        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });
                    }
                }
            }

            User returnedUser = await _userRepository.Create(user);
            return new UserResponse
            {
                Id = returnedUser.Id,
                Name = returnedUser.Name,
                LastName = returnedUser.LastName,
                Email = returnedUser.Email,
                BirthDate = returnedUser.BirthDate,
                MembershipLevel = (int?)returnedUser.MembershipLevel,
                UserRoles = returnedUser.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Score = returnedUser.Score
            };
        }

        public async Task<UserResponse> GetUserResponseById(Guid userId)
        {
            User user = await _userRepository.GetByIdWithRoles(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                MembershipLevel = (int?)user.MembershipLevel,
                UserRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Score = user.Score
            };
        }

        public async Task RegisterEntry(Guid attractionId, RegisterEntryRequest request)
        {
            Guid? qr = request.Qr;
            Guid? nfc = request.NFC;
            Guid userId = request.UserId;
            Guid? eventId = request.EventId;
            DateTime enterDate = await _dateTimeLogic.GetCurrentDateTime();

            if (qr == null && nfc == null)
                throw new ArgumentException("QR code or NFC must be provided.");

            bool isValidTicket = await _ticketLogic.ValidateTicketAsync(qr, nfc, enterDate, eventId);
            if (!isValidTicket)
                throw new ArgumentException("User does not have a valid ticket.");

            User user = await _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = await _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            if (attraction.CurrentCapacity < attraction.MaxCapacity)
            {
                user.RegisterEntry(attraction, enterDate);
                attraction.CurrentCapacity++;
                await _userRepository.Update(user);
                await _attractionRepository.Update(attraction);
            }
            else
                throw new ArgumentException("Attraction is at full capacity.");

            Event even = await _eventRepository.GetEventByAttractionAndDate(attractionId, enterDate.Date);

            await _dailyScoreLogic.AddScoreToUser(user, attraction, enterDate, even);
        }

        public async Task RegisterExit(Guid attractionId, RegisterExitRequest request)
        {
            Guid userId = request.userId;
            DateTime exitDate = await _dateTimeLogic.GetCurrentDateTime();

            User user = await _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = await _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            user.RegisterExit(attraction, exitDate);
            await _userRepository.Update(user);

            if (attraction.CurrentCapacity > 0)
            {
                attraction.CurrentCapacity--;
                await _attractionRepository.Update(attraction);
            }
        }

        public async Task<TopTenResponse> GetTopTenUsers()
        {
            List<User> users = await _userRepository.GetTopTen();
            return new TopTenResponse
            {
                TopTenUsers = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    LastName = u.LastName,
                    Email = u.Email,
                    BirthDate = u.BirthDate,
                    MembershipLevel = (int?)u.MembershipLevel,
                    UserRoles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    Score = u.Score
                }).ToList()
            };
        }

        public async Task AddRoleToUser(Guid userId, string roleName)
        {
            User user = await _userRepository.GetByIdWithRoles(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            Role? role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null)
            {
                throw new ArgumentException("Role not found.");
            }

            if (user.UserRoles.Any(ur => ur.Role == role))
            {
                throw new ArgumentException("User already has that role.");
            }

            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });

            await _userRepository.Update(user);
        }

        public async Task<UserResponse> ModifyUser(Guid userId, Guid actorUserId, ModifyUserRequest request)
        {
            if (actorUserId != userId)
            {
                throw new ForbiddenException("You cannot modify another user");
            }

            User user = await _userRepository.GetByIdWithRoles(userId);
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
                if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    bool unique = await _userRepository.IsEmailUnique(request.Email);
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
                DateTime currentDateTime = await _dateTimeLogic.GetCurrentDateTime();
                if (request.BirthDate.Value >= currentDateTime)
                    throw new ArgumentException("Birth date must be in the past");
                user.BirthDate = request.BirthDate.Value;
            }

            await _userRepository.Update(user);

            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                MembershipLevel = (int?)user.MembershipLevel,
                UserRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Score = user.Score
            };
        }

        public async Task<UserResponse> ChangeMembershipLevel(Guid userId, int membershipLevel)
        {
            if (!Enum.IsDefined(typeof(MembershipLevel), membershipLevel))
                throw new ArgumentException("Invalid membership level.");

            User user = await _userRepository.GetByIdWithRoles(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.MembershipLevel = (MembershipLevel)membershipLevel;
            await _userRepository.Update(user);

            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                MembershipLevel = (int?)user.MembershipLevel,
                UserRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Score = user.Score
            };
        }
    }
}