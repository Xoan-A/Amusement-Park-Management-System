using Domain;

namespace TestDomain
{
    [TestClass]
    public class MaintenanceScheduleTest
    {
        [TestMethod]
        public void CreateMaintenanceSchedule_ValidData_Success()
        {
            // Arrange & Act
            var schedule = new MaintenanceSchedule
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Monthly safety inspection",
                Status = MaintenanceStatus.Pending
            };

            // Assert
            Assert.IsNotNull(schedule);
            Assert.AreEqual(MaintenanceType.Inspection, schedule.MaintenanceType);
            Assert.AreEqual(MaintenanceStatus.Pending, schedule.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_EmptyString_ThrowsException()
        {
            // Arrange & Act
            var schedule = new MaintenanceSchedule
            {
                Description = ""
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_WhitespaceString_ThrowsException()
        {
            // Arrange & Act
            var schedule = new MaintenanceSchedule
            {
                Description = "   "
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_TooLong_ThrowsException()
        {
            // Arrange & Act
            var schedule = new MaintenanceSchedule
            {
                Description = new string('a', 501)
            };
        }

        [TestMethod]
        public void SetDescription_MaxLength_Success()
        {
            // Arrange & Act
            var schedule = new MaintenanceSchedule
            {
                Description = new string('a', 500)
            };

            // Assert
            Assert.AreEqual(500, schedule.Description.Length);
        }

        [TestMethod]
        public void SetScheduledDate_PastDate_IsAllowed()
        {
            // Arrange & Act
            var schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Description = "Test"
            };

            // Assert
            Assert.IsTrue(schedule.ScheduledDate < DateTime.Now);
        }

        [TestMethod]
        public void SetScheduledDate_FutureDate_Success()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(30);

            // Act
            var schedule = new MaintenanceSchedule
            {
                ScheduledDate = futureDate,
                Description = "Test"
            };

            // Assert
            Assert.AreEqual(futureDate.Date, schedule.ScheduledDate.Date);
        }

        [TestMethod]
        public void SetStatus_ValidTransition_PendingToInProgress_Success()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            // Act
            schedule.Status = MaintenanceStatus.InProgress;

            // Assert
            Assert.AreEqual(MaintenanceStatus.InProgress, schedule.Status);
        }

        [TestMethod]
        public void SetStatus_ValidTransition_InProgressToCompleted_Success()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.InProgress,
                Description = "Test"
            };

            // Act
            schedule.Status = MaintenanceStatus.Completed;

            // Assert
            Assert.AreEqual(MaintenanceStatus.Completed, schedule.Status);
        }

        [TestMethod]
        public void SetStatus_ValidTransition_PendingToCancelled_Success()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            // Act
            schedule.Status = MaintenanceStatus.Cancelled;

            // Assert
            Assert.AreEqual(MaintenanceStatus.Cancelled, schedule.Status);
        }

        [TestMethod]
        public void SetMaintenanceType_AllTypes_Success()
        {
            // Arrange & Act
            var inspection = new MaintenanceSchedule { MaintenanceType = MaintenanceType.Inspection, Description = "Test" };
            var cleaning = new MaintenanceSchedule { MaintenanceType = MaintenanceType.Cleaning, Description = "Test" };
            var repair = new MaintenanceSchedule { MaintenanceType = MaintenanceType.Repair, Description = "Test" };
            var safety = new MaintenanceSchedule { MaintenanceType = MaintenanceType.SafetyCheck, Description = "Test" };

            // Assert
            Assert.AreEqual(MaintenanceType.Inspection, inspection.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Cleaning, cleaning.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Repair, repair.MaintenanceType);
            Assert.AreEqual(MaintenanceType.SafetyCheck, safety.MaintenanceType);
        }

        [TestMethod]
        public void CreateMaintenanceSchedule_DefaultsToUtcNow_Success()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;

            // Act
            var schedule = new MaintenanceSchedule
            {
                Description = "Test",
                ScheduledDate = DateTime.Now
            };
            var afterCreate = DateTime.UtcNow;

            // Assert
            Assert.IsTrue(schedule.CreatedAt >= beforeCreate);
            Assert.IsTrue(schedule.CreatedAt <= afterCreate);
        }

        [TestMethod]
        public void SetAttractionId_ValidGuid_Success()
        {
            // Arrange
            var attractionId = Guid.NewGuid();

            // Act
            var schedule = new MaintenanceSchedule
            {
                AttractionId = attractionId,
                Description = "Test",
                ScheduledDate = DateTime.Now
            };

            // Assert
            Assert.AreEqual(attractionId, schedule.AttractionId);
        }

        [TestMethod]
        public void IsOverdue_PastScheduledDate_ReturnsTrue()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            // Act
            var isOverdue = schedule.IsOverdue();

            // Assert
            Assert.IsTrue(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_FutureScheduledDate_ReturnsFalse()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(7),
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            // Act
            var isOverdue = schedule.IsOverdue();

            // Assert
            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_CompletedStatus_ReturnsFalse()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Status = MaintenanceStatus.Completed,
                Description = "Test"
            };

            // Act
            var isOverdue = schedule.IsOverdue();

            // Assert
            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_CancelledStatus_ReturnsFalse()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Status = MaintenanceStatus.Cancelled,
                Description = "Test"
            };

            // Act
            var isOverdue = schedule.IsOverdue();

            // Assert
            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void CanComplete_PendingStatus_ReturnsTrue()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            // Act
            var canComplete = schedule.CanComplete();

            // Assert
            Assert.IsTrue(canComplete);
        }

        [TestMethod]
        public void CanComplete_InProgressStatus_ReturnsTrue()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.InProgress,
                Description = "Test"
            };

            // Act
            var canComplete = schedule.CanComplete();

            // Assert
            Assert.IsTrue(canComplete);
        }

        [TestMethod]
        public void CanComplete_CompletedStatus_ReturnsFalse()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Completed,
                Description = "Test"
            };

            // Act
            var canComplete = schedule.CanComplete();

            // Assert
            Assert.IsFalse(canComplete);
        }

        [TestMethod]
        public void CanComplete_CancelledStatus_ReturnsFalse()
        {
            // Arrange
            var schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Cancelled,
                Description = "Test"
            };

            // Act
            var canComplete = schedule.CanComplete();

            // Assert
            Assert.IsFalse(canComplete);
        }
    }
}
