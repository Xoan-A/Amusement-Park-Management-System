using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Domain;

namespace TestDomain
{
    [TestClass]
    public class UserTest
    {
        [TestMethod]
        public void User_ShouldCreateUser_WithRequiredProperties()
        {
            // Arrange & Act
            User user = new User
            {
                Name = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashedPassword123"
            };

            // Assert
            Assert.IsNotNull(user.Id);
            Assert.AreEqual("John", user.Name);
            Assert.AreEqual("Doe", user.LastName);
            Assert.AreEqual("john.doe@example.com", user.Email);
            Assert.AreEqual("hashedPassword123", user.Password);
        }

        [TestMethod]
        public void User_ShouldHaveUniqueId()
        {
            // Arrange & Act
            User user1 = new User();
            User user2 = new User();

            // Assert
            Assert.AreNotEqual(user1.Id, user2.Id);
        }

        [TestMethod]
        public void User_ShouldInitializeUserRolesCollection()
        {
            // Arrange & Act
            User user = new User();

            // Assert
            Assert.IsNotNull(user.UserRoles);
            Assert.AreEqual(0, user.UserRoles.Count);
        }

        [TestMethod]
        public void User_ShouldSupportBirthDate_ForVisitors()
        {
            // Arrange
            DateTime birthDate = new DateTime(1990, 1, 1);

            // Act
            User user = new User
            {
                Name = "Visitor",
                LastName = "User",
                Email = "visitor@example.com",
                Password = "visitorPass",
                BirthDate = birthDate
            };

            // Assert
            Assert.AreEqual(birthDate, user.BirthDate);
        }

        [TestMethod]
        public void User_ShouldSupportMembershipLevel_ForVisitors()
        {
            // Arrange & Act
            User user = new User
            {
                Name = "Visitor",
                LastName = "User",
                Email = "visitor@example.com",
                Password = "visitorPass",
                MembershipLevel = MembershipLevel.Premium
            };

            // Assert
            Assert.AreEqual(MembershipLevel.Premium, user.MembershipLevel);
        }

        [TestMethod]
        public void User_ShouldSupportMultipleRoles()
        {
            // Arrange
            User user = new User
            {
                Name = "MultiRole",
                LastName = "User",
                Email = "multi@example.com",
                Password = "pass"
            };

            Role adminRole = new Role { Id = 1, Name = "Administrator" };
            Role operatorRole = new Role { Id = 2, Name = "Operator" };

            UserRole userRole1 = new UserRole { User = user, Role = adminRole, UserId = user.Id, RoleId = adminRole.Id };
            UserRole userRole2 = new UserRole { User = user, Role = operatorRole, UserId = user.Id, RoleId = operatorRole.Id };

            // Act
            user.UserRoles.Add(userRole1);
            user.UserRoles.Add(userRole2);

            // Assert
            Assert.AreEqual(2, user.UserRoles.Count);
            Assert.IsTrue(user.UserRoles.Any(ur => ur.Role.Name == "Administrator"));
            Assert.IsTrue(user.UserRoles.Any(ur => ur.Role.Name == "Operator"));
        }

        [TestMethod]
        public void User_BirthDateAndMembershipLevel_ShouldBeNullable()
        {
            // Arrange & Act
            User user = new User
            {
                Name = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                Password = "pass"
            };

            // Assert - Should not have BirthDate or MembershipLevel set for non-visitors
            Assert.IsNull(user.BirthDate);
            Assert.IsNull(user.MembershipLevel);
        }

        [TestMethod]
        public void MembershipLevel_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.AreEqual(0, (int)MembershipLevel.Standard);
            Assert.AreEqual(1, (int)MembershipLevel.Premium);
            Assert.AreEqual(2, (int)MembershipLevel.VIP);
        }

        [TestMethod]
        public void RegisterEntry_ShouldCreateNewVisitorReportWhenNoneExists()
        {
            User user = new User
            {
                Name = "John",
                LastName = "Doe"
            };
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            Attraction attraction = new Attraction
            {
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };

            user.RegisterEntry(attraction, enterDate);

            Assert.AreEqual(1, user.VisitorReports.Count);
            Assert.AreEqual(enterDate.Date, user.VisitorReports[0].Date.Date);
            Assert.AreEqual(1, user.VisitorReports[0].Reports.Count);
            Assert.AreEqual(enterDate, user.VisitorReports[0].Reports[0].EnterDate);
        }

        [TestMethod]
        public void RegisterEntry_ShouldAddReportToExistingVisitorReport()
        {
            DateTime enterDate1 = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime enterDate2 = new DateTime(2025, 10, 1, 14, 0, 0);
            Attraction attraction1 = new Attraction
            {
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };
            Attraction attraction2 = new Attraction
            {
                Name = "Simulator",
                Type = AttractionType.Simulator
            };
            User user = new User
            {
                Name = "John",
                LastName = "Doe"
            };

            user.RegisterEntry(attraction1, enterDate1);
            user.RegisterEntry(attraction2, enterDate2);

            Assert.AreEqual(1, user.VisitorReports.Count);
            Assert.AreEqual(2, user.VisitorReports[0].Reports.Count);
            Assert.AreEqual(enterDate1, user.VisitorReports[0].Reports[0].EnterDate);
            Assert.AreEqual(enterDate2, user.VisitorReports[0].Reports[1].EnterDate);
        }

        [TestMethod]
        public void RegisterEntry_ShouldCreateSeparateVisitorReportsForDifferentDays()
        {
            DateTime enterDate1 = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime enterDate2 = new DateTime(2025, 10, 2, 10, 0, 0);
            Attraction attraction = new Attraction
            {
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };
            User user = new User
            {
                Name = "John",
                LastName = "Doe"
            };

            user.RegisterEntry(attraction, enterDate1);
            user.RegisterEntry(attraction, enterDate2);

            Assert.AreEqual(2, user.VisitorReports.Count);
            Assert.AreEqual(enterDate1.Date, user.VisitorReports[0].Date.Date);
            Assert.AreEqual(enterDate2.Date, user.VisitorReports[1].Date.Date);
        }

        [TestMethod]
        public void RegisterExit_ShouldSetExitTimeForReport()
        {
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);
            Attraction attraction = new Attraction
            {
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };
            User user = new User
            {
                Name = "John",
                LastName = "Doe"
            };
            user.RegisterEntry(attraction, enterDate);

            user.RegisterExit(attraction, exitDate);

            Assert.AreEqual(exitDate, user.VisitorReports[0].Reports[0].ExitDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterExit_ShouldThrowExceptionWhenNoVisitorReportExists()
        {
            Attraction attraction = new Attraction
            {
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);
            User user = new User
            {
                Name = "John",
                LastName = "Doe"
            };

            user.RegisterExit(attraction, exitDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RegisterExit_ShouldThrowExceptionWhenNoReportWithNoExitDateExists()
        {
            DateTime enterDate = new DateTime(2025, 10, 1, 10, 0, 0);
            DateTime exitDate = new DateTime(2025, 10, 1, 15, 30, 0);
            DateTime exitDate2 = new DateTime(2025, 10, 1, 15, 30, 0);
            Attraction attraction = new Attraction
            {
                Name = "Roller Coaster",
                Type = AttractionType.RollerCoaster
            };
            User user = new User
            {
                Name = "John",
                LastName = "Doe"
            };
            user.RegisterEntry(attraction, enterDate);

            user.RegisterExit(attraction, exitDate);
            user.RegisterExit(attraction, exitDate2);
        }
    }
}
