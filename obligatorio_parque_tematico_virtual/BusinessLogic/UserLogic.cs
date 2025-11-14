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

        public UserResponse RegisterVisitor(RegisterVisitorRequest request)
        {
            string name = request.Name;
            string lastName = request.LastName;
            string email = request.Email;
            string password = request.Password;
            DateTime birthDate = request.BirthDate;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Name, last name, email, and password must be provided.");

            if (!ValidateEmail(email))
                throw new ArgumentException("Invalid email format.");

            DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();

            if (birthDate >= currentDateTime)
                throw new ArgumentException("Birth date cannot be after today.");

            if (!_userRepository.IsEmailUnique(email))
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

            Role? visitorRole = _roleRepository.GetByName(Role.VISITOR);
            if (visitorRole != null)
            {
                visitor.UserRoles.Add(new UserRole
                { UserId = visitor.Id, RoleId = visitorRole.Id, Role = visitorRole });
            }

            User res = _userRepository.Create(visitor);

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

        public UserResponse CreateUser(CreateUserRequest request)
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

            if (!ValidateEmail(email))
                throw new ArgumentException("Invalid email format.");

            if (!_userRepository.IsEmailUnique(email))
            {
                throw new ArgumentException("Email is already in use.");
            }

            if (birthDate.HasValue)
            {
                DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
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
                if (Enum.TryParse<MembershipLevel>(membershipLevel, true, out MembershipLevel parsedLevel) &&
                    Enum.IsDefined(typeof(MembershipLevel), parsedLevel))
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
                    Role? role = _roleRepository.GetByName(roleName);
                    if (role != null)
                    {
                        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });
                    }
                }
            }

            User returnedUser = _userRepository.Create(user);
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

        public UserResponse GetUserResponseById(Guid userId)
        {
            User user = _userRepository.GetByIdWithRoles(userId);
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

        public void RegisterEntry(Guid attractionId, RegisterEntryRequest request)
        {
            Guid? qr = request.Qr;
            Guid? nfc = request.NFC;
            Guid userId = request.UserId;
            Guid? eventId = request.EventId;
            DateTime enterDate = _dateTimeLogic.GetCurrentDateTime();

            if (qr == null && nfc == null)
                throw new ArgumentException("QR code or NFC must be provided.");

            Attraction attraction = _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            bool isValidTicket = _ticketLogic.ValidateTicket(qr, nfc, enterDate, eventId, attractionId);
            if (!isValidTicket)
                throw new ArgumentException("User does not have a valid ticket.");

            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");


            if (attraction.CurrentCapacity < attraction.MaxCapacity)
            {
                user.RegisterEntry(attraction, enterDate);
                attraction.CurrentCapacity++;
                _userRepository.Update(user);
                _attractionRepository.Update(attraction);
            }
            else
                throw new ArgumentException("Attraction is at full capacity.");

            Event even = _eventRepository.GetEventByAttractionAndDate(attractionId, enterDate.Date);

            _dailyScoreLogic.AddScoreToUser(user, attraction, enterDate, even);
        }

        public void RegisterExit(Guid attractionId, RegisterExitRequest request)
        {
            Guid userId = request.userId;
            DateTime exitDate = _dateTimeLogic.GetCurrentDateTime();

            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            user.RegisterExit(attraction, exitDate);
            _userRepository.Update(user);

            if (attraction.CurrentCapacity > 0)
            {
                attraction.CurrentCapacity--;
                _attractionRepository.Update(attraction);
            }
        }

        public TopTenResponse GetTopTenUsers()
        {
            List<User> users = _userRepository.GetTopTen();
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
                if (!ValidateEmail(request.Email))
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
                DateTime currentDateTime = _dateTimeLogic.GetCurrentDateTime();
                if (request.BirthDate.Value >= currentDateTime)
                    throw new ArgumentException("Birth date must be in the past");
                user.BirthDate = request.BirthDate.Value;
            }

            _userRepository.Update(user);

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

        public UserResponse ChangeMembershipLevel(Guid userId, int membershipLevel)
        {
            if (!Enum.IsDefined(typeof(MembershipLevel), membershipLevel))
                throw new ArgumentException("Invalid membership level.");

            User user = _userRepository.GetByIdWithRoles(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.MembershipLevel = (MembershipLevel)membershipLevel;
            _userRepository.Update(user);

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

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            if (!email.Contains("@"))
                return false;

            int atIndex = email.IndexOf("@");
            if (atIndex == 0 || atIndex == email.Length - 1)
                return false;

            if (email.IndexOf("@", atIndex + 1) != -1)
                return false;

            string domain = email.Substring(atIndex + 1);
            if (!domain.Contains("."))
                return false;

            if (domain.StartsWith(".") || domain.EndsWith("."))
                return false;

            return true;
        }
    }
}