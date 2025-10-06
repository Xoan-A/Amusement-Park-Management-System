using System;
using System.Linq;
using Domain;
using IBusinessLogic;
using IDataAccess;
using Models.Out;

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

        public User RegisterVisitor(string name, string lastName, string email, string password, DateTime birthDate)
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

            if (!_userRepository.IsEmailUnique(email))
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
                visitor.UserRoles.Add(new UserRole { UserId = visitor.Id, RoleId = visitorRole.Id, Role = visitorRole });
            }

            return _userRepository.Create(visitor);
        }

        public User CreateUser(string name, string lastName, string email, string password, string[] roles)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(lastName) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (!_userRepository.IsEmailUnique(email))
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

            if (roles != null && roles.Length > 0)
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

            return _userRepository.Create(user);
        }

        public async Task RegisterEntry(Guid userId, Guid attractionId, DateTime enterDate, Guid? qr, Guid? nfc,
            int? eventId)
        {
            if (qr == null && nfc == null)
                throw new ArgumentException("QR code or NFC must be provided.");

            bool isValidTicket = await _ticketLogic.ValidateTicketAsync(qr, nfc, enterDate, eventId);
            if (!isValidTicket)
                throw new ArgumentException("User does not have a valid ticket.");

            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = await _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            if (attraction.CurrentCapacity < attraction.MaxCapacity)
            {
                user.RegisterEntry(attraction, enterDate);
                attraction.CurrentCapacity++;
            }
            else
                throw new ArgumentException("Attraction is at full capacity.");

            Event even = await _eventRepository.GetEventByAttractionAndDate(attractionId, enterDate.Date);

            bool isEvent = even != null;

            var strategyRequest = new StrategyRequest
            {
                User = user,
                Attraction = attraction,
                IsSepcialEvent = isEvent,
                EnterDate = enterDate,
            };

            int score = _activeStrategy.CalculateScore(strategyRequest);

            user.Score += score;
        }

        public async Task RegisterExit(Guid userId, Guid attractionId, DateTime exitDate)
        {
            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");

            Attraction attraction = await _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");

            user.RegisterExit(attraction, exitDate);

            if (attraction.CurrentCapacity > 0)
                attraction.CurrentCapacity--;
        }

        public async Task<TopTenResponse> GetTopTenUsers()
        {
            TopTenResponse result = new TopTenResponse();
            result.TopTenUsers = await _userRepository.GetTopTen();
            return result;
        }
    }
}