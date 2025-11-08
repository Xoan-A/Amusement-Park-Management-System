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
                Description = "Monthly safety inspection",
                Status = MaintenanceStatus.Pending
            };

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
            DateTime currentDateTime = DateTime.Now;
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = currentDateTime.AddDays(-1),
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue(currentDateTime);

            Assert.IsTrue(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_FutureScheduledDate_ReturnsFalse()
        {
            DateTime currentDateTime = DateTime.Now;
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = currentDateTime.AddDays(7),
                Status = MaintenanceStatus.Pending,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue(currentDateTime);

            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_CompletedStatus_ReturnsFalse()
        {
            DateTime currentDateTime = DateTime.Now;
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = currentDateTime.AddDays(-1),
                Status = MaintenanceStatus.Completed,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue(currentDateTime);

            Assert.IsFalse(isOverdue);
        }

        [TestMethod]
        public void IsOverdue_CancelledStatus_ReturnsFalse()
        {
            DateTime currentDateTime = DateTime.Now;
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                ScheduledDate = currentDateTime.AddDays(-1),
                Status = MaintenanceStatus.Cancelled,
                Description = "Test"
            };

            bool isOverdue = schedule.IsOverdue(currentDateTime);

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

        [TestMethod]
        public void SetEstimatedDuration_ValidValue_Success()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = "Test",
                EstimatedDuration = 120
            };

            Assert.AreEqual(120, schedule.EstimatedDuration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetEstimatedDuration_ZeroValue_ThrowsException()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = "Test",
                EstimatedDuration = 0
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetEstimatedDuration_NegativeValue_ThrowsException()
        {
            MaintenanceSchedule schedule = new MaintenanceSchedule
            {
                Description = "Test",
                EstimatedDuration = -10
            };
        }
    }
}