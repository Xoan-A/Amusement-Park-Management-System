using System;
using System.Collections.Generic;
using Domain;
using Models.In;
using Models.Out;

namespace IBusinessLogic
{
    public interface IUserLogic
    {
        User RegisterVisitor(string name, string lastName, string email, string password, DateTime birthDate);
        User CreateUser(string name, string lastName, string email, string password, string[] roles);
        Task RegisterEntry(Guid userId, Guid attractionId, DateTime enterDate, Guid? qr, Guid? nfc, Guid? eventId);
        Task RegisterExit(Guid userId, Guid attractionId, DateTime exitDate);
        Task<TopTenResponse> GetTopTenUsers();
        Task AddRoleToUser(Guid userId, string role);
    }
}