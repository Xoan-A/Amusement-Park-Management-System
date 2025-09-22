using System;
using Domain;

namespace IBusinessLogic
{
    public interface IUserLogic
    {
        Visitor RegisterVisitor(string name, string lastName, string email, string password, DateTime birthDate);
    }
}