using System;
using Domain;
using Models.In;

namespace IBusinessLogic
{
    public interface IUserLogic
    {
        Visitor RegisterVisitor(string name, string lastName, string email, string password, DateTime birthDate);
        Task RegisterEntry(Guid userId, Guid attractionId, DateTime entrerDate, Guid? qr, Guid? nfc, int? eventId);
        Task RegisterExit(Guid userId, Guid attractionId, DateTime exitDate);
    }
}