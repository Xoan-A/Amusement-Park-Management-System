using Domain;

namespace TestDomain
{
    [TestClass]
    public class MaintenanceRecordTest
    {
        [TestMethod]
        public void CreateMaintenanceRecord_ValidData_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                PerformedDate = DateTime.Now,
                PerformedBy = Guid.NewGuid(),
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Completed monthly inspection",
                Notes = "All systems operational"
            };

            Assert.IsNotNull(record);
            Assert.AreEqual(MaintenanceType.Inspection, record.MaintenanceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_EmptyString_ThrowsException()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = ""
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_WhitespaceString_ThrowsException()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "   "
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_TooLong_ThrowsException()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = new string('a', 501)
            };
        }

        [TestMethod]
        public void SetDescription_MaxLength_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = new string('a', 500)
            };

            Assert.AreEqual(500, record.Description.Length);
        }

        [TestMethod]
        public void SetNotes_EmptyString_IsAllowed()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = ""
            };

            Assert.AreEqual("", record.Notes);
        }

        [TestMethod]
        public void SetNotes_NullValue_IsAllowed()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = null
            };

            Assert.IsNull(record.Notes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetNotes_TooLong_ThrowsException()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = new string('a', 1001)
            };
        }

        [TestMethod]
        public void SetNotes_MaxLength_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = new string('a', 1000)
            };

            Assert.AreEqual(1000, record.Notes.Length);
        }

        [TestMethod]
        public void SetDuration_PositiveValue_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Duration = TimeSpan.FromHours(2)
            };

            Assert.AreEqual(TimeSpan.FromHours(2), record.Duration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDuration_NegativeValue_ThrowsException()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Duration = TimeSpan.FromHours(-1)
            };
        }

        [TestMethod]
        public void SetDuration_ZeroValue_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                Duration = TimeSpan.Zero
            };

            Assert.AreEqual(TimeSpan.Zero, record.Duration);
        }

        [TestMethod]
        public void CreateMaintenanceRecord_CreatedAtCanBeSet_Success()
        {
            DateTime createdAt = new DateTime(2025, 10, 15, 10, 30, 0);

            MaintenanceRecord record = new MaintenanceRecord
            {
                Description = "Test",
                PerformedDate = DateTime.Now,
                CreatedAt = createdAt
            };

            Assert.AreEqual(createdAt, record.CreatedAt);
        }

        [TestMethod]
        public void SetMaintenanceScheduleId_ValidGuid_Success()
        {
            Guid scheduleId = Guid.NewGuid();

            MaintenanceRecord record = new MaintenanceRecord
            {
                MaintenanceScheduleId = scheduleId,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            Assert.AreEqual(scheduleId, record.MaintenanceScheduleId);
        }

        [TestMethod]
        public void SetMaintenanceScheduleId_Null_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                MaintenanceScheduleId = null,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            Assert.IsNull(record.MaintenanceScheduleId);
        }

        [TestMethod]
        public void SetPerformedBy_ValidGuid_Success()
        {
            Guid operatorId = Guid.NewGuid();

            MaintenanceRecord record = new MaintenanceRecord
            {
                PerformedBy = operatorId,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            Assert.AreEqual(operatorId, record.PerformedBy);
        }

        [TestMethod]
        public void SetMaintenanceType_AllTypes_Success()
        {
            MaintenanceRecord inspection = new MaintenanceRecord
            { MaintenanceType = MaintenanceType.Inspection, Description = "Test" };
            MaintenanceRecord cleaning = new MaintenanceRecord
            { MaintenanceType = MaintenanceType.Cleaning, Description = "Test" };
            MaintenanceRecord repair = new MaintenanceRecord
            { MaintenanceType = MaintenanceType.Repair, Description = "Test" };
            MaintenanceRecord safety = new MaintenanceRecord
            { MaintenanceType = MaintenanceType.SafetyCheck, Description = "Test" };

            Assert.AreEqual(MaintenanceType.Inspection, inspection.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Cleaning, cleaning.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Repair, repair.MaintenanceType);
            Assert.AreEqual(MaintenanceType.SafetyCheck, safety.MaintenanceType);
        }

        [TestMethod]
        public void SetPerformedDate_PastDate_Success()
        {
            DateTime pastDate = DateTime.Now.AddDays(-7);

            MaintenanceRecord record = new MaintenanceRecord
            {
                PerformedDate = pastDate,
                Description = "Test"
            };

            Assert.AreEqual(pastDate.Date, record.PerformedDate.Date);
        }

        [TestMethod]
        public void SetPerformedDate_FutureDate_IsAllowed()
        {
            DateTime futureDate = DateTime.Now.AddDays(1);

            MaintenanceRecord record = new MaintenanceRecord
            {
                PerformedDate = futureDate,
                Description = "Test"
            };

            Assert.AreEqual(futureDate.Date, record.PerformedDate.Date);
        }

        [TestMethod]
        public void CreateMaintenanceRecord_WithScheduleLink_Success()
        {
            Guid scheduleId = Guid.NewGuid();
            Guid attractionId = Guid.NewGuid();
            Guid operatorId = Guid.NewGuid();

            MaintenanceRecord record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                MaintenanceScheduleId = scheduleId,
                AttractionId = attractionId,
                PerformedBy = operatorId,
                PerformedDate = DateTime.Now,
                MaintenanceType = MaintenanceType.Repair,
                Description = "Fixed hydraulic system",
                Notes = "Replaced seals",
                Duration = TimeSpan.FromHours(3)
            };

            Assert.IsNotNull(record);
            Assert.AreEqual(scheduleId, record.MaintenanceScheduleId);
            Assert.AreEqual(attractionId, record.AttractionId);
            Assert.AreEqual(operatorId, record.PerformedBy);
            Assert.AreEqual(TimeSpan.FromHours(3), record.Duration);
        }

        [TestMethod]
        public void CreateMaintenanceRecord_UnscheduledMaintenance_Success()
        {
            MaintenanceRecord record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                MaintenanceScheduleId = null,
                AttractionId = Guid.NewGuid(),
                PerformedBy = Guid.NewGuid(),
                PerformedDate = DateTime.Now,
                MaintenanceType = MaintenanceType.Repair,
                Description = "Emergency repair",
                Notes = "Unscheduled maintenance due to malfunction"
            };

            Assert.IsNotNull(record);
            Assert.IsNull(record.MaintenanceScheduleId);
        }

        [TestMethod]
        public void SetAttractionId_ValidGuid_Success()
        {
            Guid attractionId = Guid.NewGuid();

            MaintenanceRecord record = new MaintenanceRecord
            {
                AttractionId = attractionId,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            Assert.AreEqual(attractionId, record.AttractionId);
        }
    }
}