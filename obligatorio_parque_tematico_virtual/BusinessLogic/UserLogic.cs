using System;
using Domain;
using IBusinessLogic;
using IDataAccess;

namespace BusinessLogic
{
    public class UserLogic : IUserLogic
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IAttractionRepository _attractionRepository;

        public UserLogic(IUserRepository userRepository, IPasswordService passwordService, IAttractionRepository attractionRepository)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _attractionRepository = attractionRepository;
        }

        public Visitor RegisterVisitor(string name, string lastName, string email, string password, DateTime birthDate)
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

            string hashedPassword = _passwordService.HashPassword(password);

            Visitor visitor = new Visitor
            {
                Name = name,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Standard
            };

            return _userRepository.Create(visitor) as Visitor;
        }
        
        public async Task RegisterEntry(Guid userId, Guid attractionId, DateTime enterDate)
        {
            User user = _userRepository.GetById(userId);
            if (user == null)
                throw new ArgumentException("User not found.");
            
            Attraction attraction = await _attractionRepository.GetById(attractionId);
            if (attraction == null)
                throw new ArgumentException("Attraction not found.");
          
            user.RegisterEntry(attraction, enterDate);
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
        }
    }
}