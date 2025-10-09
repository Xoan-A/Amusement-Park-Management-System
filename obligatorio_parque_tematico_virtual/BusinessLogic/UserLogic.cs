using System;
using System.Linq;
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
        private readonly IActiveStrategy _activeStrategy;

        public UserLogic(IUserRepository userRepository, IPasswordLogic passwordLogic,
            IAttractionRepository attractionRepository, ITicketLogic ticketLogic, IRoleRepository roleRepository,
            IEventRepository eventRepository, IActiveStrategy activeStrategy)
        {
            _userRepository = userRepository;
            _passwordLogic = passwordLogic;
            _attractionRepository = attractionRepository;
            _ticketLogic = ticketLogic;
            _roleRepository = roleRepository;
            _eventRepository = eventRepository;
            _activeStrategy = activeStrategy;
        }

        public async Task<User> RegisterVisitor(string name, string lastName, string email, string password,
            DateTime birthDate)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (birthDate >= DateTime.Now)
            {
                return null;
            }

            if (!await _userRepository.IsEmailUnique(email))
            {
                return null;
            }

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

            Role visitorRole = _roleRepository.GetByName(Role.VISITOR);
            if (visitorRole != null)
            {
                visitor.UserRoles.Add(new UserRole
                { UserId = visitor.Id, RoleId = visitorRole.Id, Role = visitorRole });
            }

            return await _userRepository.Create(visitor);
        }

        public async Task<UserResponse> CreateUser(CreateUserRequest request)
        {
            string name = request.Name;
            string lastName = request.LastName;
            string email = request.Email;
            string password = request.Password;
            List<string> roles = request.Roles;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (!await _userRepository.IsEmailUnique(email))
            {
                return null;
            }

            string hashedPassword = _passwordLogic.HashPassword(password);

            User user = new User
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword
            };

            if (roles != null && roles.Count > 0)
            {
                foreach (string roleName in roles)
                {
                    Role role = _roleRepository.GetByName(roleName);
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

        public async Task<User> GetUserById(Guid userId)
        {
            User user = await _userRepository.GetByIdWithRoles(userId);
            return user;
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

        public async Task RegisterEntry(Guid userId, Guid attractionId, DateTime enterDate, Guid? qr, Guid? nfc,
            Guid? eventId)
        {
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
                await _attractionRepository.Update(attraction);
            }
            else
                throw new ArgumentException("Attraction is at full capacity.");

            Event even = await _eventRepository.GetEventByAttractionAndDate(attractionId, enterDate.Date);

            bool isEvent = even != null;

            var strategyRequest = new StrategyRequest
            {
                UserId = user.Id,
                AttractionId = attraction.Id,
                IsSepcialEvent = isEvent,
            };

            int score = await _activeStrategy.CalculateScore(user, attraction, strategyRequest);

            user.Score += score;
            await _userRepository.Update(user);
        }

        public async Task RegisterExit(Guid userId, Guid attractionId, DateTime exitDate)
        {
            User user = await _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = await _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            user.RegisterExit(attraction, exitDate);

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

            Role role = _roleRepository.GetByName(roleName);
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

        public async Task<UserResponse> ModifyUser(Guid userId, string? actorSubClaim, ModifyUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(actorSubClaim) || !Guid.TryParse(actorSubClaim, out var actorUserId))
            {
                throw new UnauthorizedException("Invalid token");
            }

            if (actorUserId != userId)
            {
                throw new ForbiddenException("You cannot modify another user");
            }

            User user = await _userRepository.GetByIdWithRoles(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name cannot be empty");
            user.Name = request.Name;

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ArgumentException("Last name cannot be empty");
            user.LastName = request.LastName;

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email cannot be empty");
            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                bool unique = await _userRepository.IsEmailUnique(request.Email);
                if (!unique)
                    throw new ArgumentException("Email must be unique");
                user.Email = request.Email;
            }

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password cannot be empty");
            string hashedPassword = _passwordLogic.HashPassword(request.Password);
            user.Password = hashedPassword;

            if (request.BirthDate.HasValue)
            {
                if (request.BirthDate.Value >= DateTime.Now)
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
    }
}