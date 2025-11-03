using Domain;

namespace TestDomain
{
    [TestClass]
    public class MaintenanceScheduleTest
    {
        [TestMethod]
        public void CreateMaintenanceSchedule_ValidData_Success()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                ScheduledDate = DateTime.Now.AddDays(7),
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Monthly safety inspection",
                Status = MaintenanceStatus.Pending
            };

            Assert.IsNotNull(schedule);
            Assert.AreEqual(MaintenanceType.Inspection, schedule.MaintenanceType);
            Assert.AreEqual(MaintenanceStatus.Pending, schedule.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_EmptyString_ThrowsException()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = ""
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_WhitespaceString_ThrowsException()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = "   "
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_TooLong_ThrowsException()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = new string('a', 501)
            };
        }

        [TestMethod]
        public void SetDescription_MaxLength_Success()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = new string('a', 500)
            };

            Assert.AreEqual(500, schedule.Description.Length);
        }

        [TestMethod]
        public void SetScheduledDate_PastDate_IsAllowed()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Description = "Test"
            };

            Assert.IsTrue(schedule.ScheduledDate < DateTime.Now);
        }

        [TestMethod]
        public void SetScheduledDate_FutureDate_Success()
        {
            DateTime futureDate = DateTime.Now.AddDays(30);

            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = futureDate,
                Description = "Test"
            };

            Assert.AreEqual(futureDate.Date, schedule.ScheduledDate.Date);
        }

        [TestMethod]
        public void SetStatus_ValidTransition_PendingToInProgress_Success()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            schedule.Status = MaintenanceStatus.InProgress;

            Assert.AreEqual(MaintenanceStatus.InProgress, schedule.Status);
        }

        [TestMethod]
        public void SetStatus_ValidTransition_InProgressToCompleted_Success()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.InProgress,
                Description = "Test"
            };

            schedule.Status = MaintenanceStatus.Completed;

            Assert.AreEqual(MaintenanceStatus.Completed, schedule.Status);
        }

        [TestMethod]
        public void SetStatus_ValidTransition_PendingToCancelled_Success()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            schedule.Status = MaintenanceStatus.Cancelled;

            Assert.AreEqual(MaintenanceStatus.Cancelled, schedule.Status);
        }

        [TestMethod]
        public void SetMaintenanceType_AllTypes_Success()
        {
            MaintenanceSchedule inspection = new MaintenanceSchedule
                { MaintenanceType = MaintenanceType.Inspection, Description = "Test" };
            MaintenanceSchedule cleaning = new MaintenanceSchedule
                { MaintenanceType = MaintenanceType.Cleaning, Description = "Test" };
            MaintenanceSchedule repair = new MaintenanceSchedule
                { MaintenanceType = MaintenanceType.Repair, Description = "Test" };
            MaintenanceSchedule safety = new MaintenanceSchedule
                { MaintenanceType = MaintenanceType.SafetyCheck, Description = "Test" };

            Assert.AreEqual(MaintenanceType.Inspection, inspection.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Cleaning, cleaning.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Repair, repair.MaintenanceType);
            Assert.AreEqual(MaintenanceType.SafetyCheck, safety.MaintenanceType);
        }

        [TestMethod]
        public void CreateMaintenanceSchedule_DefaultsToUtcNow_Success()
        {
            DateTime beforeCreate = DateTime.UtcNow;

            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = "Test",
                ScheduledDate = DateTime.Now
            };
            DateTime afterCreate = DateTime.UtcNow;

            Assert.IsTrue(schedule.CreatedAt >= beforeCreate);
            Assert.IsTrue(schedule.CreatedAt <= afterCreate);
        }

        [TestMethod]
        public void SetAttractionId_ValidGuid_Success()
        {
            Guid attractionId = Guid.NewGuid();

            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                AttractionId = attractionId,
                Description = "Test",
                ScheduledDate = DateTime.Now
            };

            Assert.AreEqual(attractionId, schedule.AttractionId);
        }

        [TestMethod]
        public void IsOverdue_PastScheduledDate_ReturnsTrue()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue();

            Assert.IsTrue(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_FutureScheduledDate_ReturnsFalse()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(7),
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue();

            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_CompletedStatus_ReturnsFalse()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Status = MaintenanceStatus.Completed,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue();

            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_CancelledStatus_ReturnsFalse()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = DateTime.Now.AddDays(-1),
                Status = MaintenanceStatus.Cancelled,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue();

            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void CanComplete_PendingStatus_ReturnsTrue()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            bool canComplete = schedule.CanComplete();

            Assert.IsTrue(canComplete);
        }

        [TestMethod]
        public void CanComplete_InProgressStatus_ReturnsTrue()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.InProgress,
                Description = "Test"
            };

            bool canComplete = schedule.CanComplete();

            Assert.IsTrue(canComplete);
        }

        [TestMethod]
        public void CanComplete_CompletedStatus_ReturnsFalse()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Completed,
                Description = "Test"
            };

            bool canComplete = schedule.CanComplete();

            Assert.IsFalse(canComplete);
        }

        [TestMethod]
        public void CanComplete_CancelledStatus_ReturnsFalse()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Status = MaintenanceStatus.Cancelled,
                Description = "Test"
            };

            bool canComplete = schedule.CanComplete();

            Assert.IsFalse(canComplete);
        }
    }
}