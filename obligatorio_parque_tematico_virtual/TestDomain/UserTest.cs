using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Domain;

namespace TestDomain
{
    [TestClass]
    public class UserTest
    {
        [TestMethod]
        public void User_ShouldHaveRequiredProperties()
        {
            User user = new Administrator();

            user.Id = Guid.NewGuid();
            user.Name = "John";
            user.LastName = "Doe";
            user.Email = "john.doe@example.com";
            user.Password = "hashedPassword123";

            Assert.IsNotNull(user.Id);
            Assert.AreEqual("John", user.Name);
            Assert.AreEqual("Doe", user.LastName);
            Assert.AreEqual("john.doe@example.com", user.Email);
            Assert.AreEqual("hashedPassword123", user.Password);
        }

        [TestMethod]
        public void Administrator_ShouldInheritFromUser()
        {
            Administrator admin = new Administrator
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                Password = "adminPass"
            };

            Assert.IsInstanceOfType(admin, typeof(User));
            Assert.IsNotNull(admin.Id);
            Assert.AreEqual("Admin", admin.Name);
        }

        [TestMethod]
        public void Operator_ShouldInheritFromUser()
        {
            Operator op = new Operator
            {
                Id = Guid.NewGuid(),
                Name = "Operator",
                LastName = "User",
                Email = "operator@example.com",
                Password = "operatorPass"
            };

            Assert.IsInstanceOfType(op, typeof(User));
            Assert.IsNotNull(op.Id);
            Assert.AreEqual("Operator", op.Name);
        }

        [TestMethod]
        public void Visitor_ShouldInheritFromUser_WithAdditionalProperties()
        {
            DateTime birthDate = new DateTime(1990, 1, 1);
            Visitor visitor = new Visitor
            {
                Id = Guid.NewGuid(),
                Name = "Visitor",
                LastName = "User",
                Email = "visitor@example.com",
                Password = "visitorPass",
                BirthDate = birthDate,
                MembershipLevel = MembershipLevel.Premium
            };

            Assert.IsInstanceOfType(visitor, typeof(User));
            Assert.IsNotNull(visitor.Id);
            Assert.AreEqual("Visitor", visitor.Name);
            Assert.AreEqual(birthDate, visitor.BirthDate);
            Assert.AreEqual(MembershipLevel.Premium, visitor.MembershipLevel);
        }

        [TestMethod]
        public void MembershipLevel_ShouldHaveCorrectValues()
        {
            Assert.AreEqual(0, (int)MembershipLevel.Standard);
            Assert.AreEqual(1, (int)MembershipLevel.Premium);
            Assert.AreEqual(2, (int)MembershipLevel.VIP);
        }

        [TestMethod]
        public void Visitor_DefaultMembershipLevel_ShouldBeStandard()
        {
            Visitor visitor = new Visitor();
            Assert.AreEqual(MembershipLevel.Standard, visitor.MembershipLevel);
        }

        [TestMethod]
        public void User_ShouldHaveUniqueId()
        {
            Administrator user1 = new Administrator();
            Administrator user2 = new Administrator();

            Assert.AreNotEqual(user1.Id, user2.Id);
        }
    }
}