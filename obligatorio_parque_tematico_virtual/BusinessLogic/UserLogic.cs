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

        public UserLogic(IUserRepository userRepository, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
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
    }
}