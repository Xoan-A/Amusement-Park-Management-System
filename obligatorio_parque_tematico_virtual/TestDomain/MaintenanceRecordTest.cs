using Domain;

namespace TestDomain
{
    [TestClass]
    public class MaintenanceRecordTest
    {
        [TestMethod]
        public void CreateMaintenanceRecord_ValidData_Success()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Id = Guid.NewGuid(),
                AttractionId = Guid.NewGuid(),
                PerformedDate = DateTime.Now,
                PerformedBy = Guid.NewGuid(),
                MaintenanceType = MaintenanceType.Inspection,
                Description = "Completed monthly inspection",
                Notes = "All systems operational"
            };

            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(MaintenanceType.Inspection, record.MaintenanceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_EmptyString_ThrowsException()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = ""
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_WhitespaceString_ThrowsException()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "   "
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDescription_TooLong_ThrowsException()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = new string('a', 501)
            };
        }

        [TestMethod]
        public void SetDescription_MaxLength_Success()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = new string('a', 500)
            };

            // Assert
            Assert.AreEqual(500, record.Description.Length);}

        [TestMethod]
        public void SetNotes_EmptyString_IsAllowed()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = ""
            };

            // Assert
            Assert.AreEqual("", record.Notes);
        }

        [TestMethod]
        public void SetNotes_NullValue_IsAllowed()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = null
            };

            // Assert
            Assert.IsNull(record.Notes);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetNotes_TooLong_ThrowsException()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = new string('a', 1001)
            };
        }

        [TestMethod]
        public void SetNotes_MaxLength_Success()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Notes = new string('a', 1000)
            };

            // Assert
            Assert.AreEqual(1000, record.Notes.Length);
        }

        [TestMethod]
        public void SetDuration_PositiveValue_Success()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Duration = TimeSpan.FromHours(2)
            };

            // Assert
            Assert.AreEqual(TimeSpan.FromHours(2), record.Duration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetDuration_NegativeValue_ThrowsException()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Duration = TimeSpan.FromHours(-1)
            };
        }

        [TestMethod]
        public void SetDuration_ZeroValue_Success()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                Duration = TimeSpan.Zero
            };

            // Assert
            Assert.AreEqual(TimeSpan.Zero, record.Duration);
        }

        [TestMethod]
        public void CreateMaintenanceRecord_DefaultsToUtcNow_Success()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;

            // Act
            var record = new MaintenanceRecord
            {
                Description = "Test",
                PerformedDate = DateTime.Now
            };
            var afterCreate = DateTime.UtcNow;

            // Assert
            Assert.IsTrue(record.CreatedAt >= beforeCreate);
            Assert.IsTrue(record.CreatedAt <= afterCreate);
        }

        [TestMethod]
        public void SetMaintenanceScheduleId_ValidGuid_Success()
        {
            // Arrange
            var scheduleId = Guid.NewGuid();

            // Act
            var record = new MaintenanceRecord
            {
                MaintenanceScheduleId = scheduleId,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            // Assert
            Assert.AreEqual(scheduleId, record.MaintenanceScheduleId);
        }

        [TestMethod]
        public void SetMaintenanceScheduleId_Null_Success()
        {
            // Arrange & Act
            var record = new MaintenanceRecord
            {
                MaintenanceScheduleId = null,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            // Assert
            Assert.IsNull(record.MaintenanceScheduleId);
        }

        [TestMethod]
        public void SetPerformedBy_ValidGuid_Success()
        {
            // Arrange
            var operatorId = Guid.NewGuid();

            // Act
            var record = new MaintenanceRecord
            {
                PerformedBy = operatorId,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            // Assert
            Assert.AreEqual(operatorId, record.PerformedBy);
        }

        [TestMethod]
        public void SetMaintenanceType_AllTypes_Success()
        {
            // Arrange & Act
            var inspection = new MaintenanceRecord { MaintenanceType = MaintenanceType.Inspection, Description = "Test" };
            var cleaning = new MaintenanceRecord { MaintenanceType = MaintenanceType.Cleaning, Description = "Test" };
            var repair = new MaintenanceRecord { MaintenanceType = MaintenanceType.Repair, Description = "Test" };
            var safety = new MaintenanceRecord { MaintenanceType = MaintenanceType.SafetyCheck, Description = "Test" };

            // Assert
            Assert.AreEqual(MaintenanceType.Inspection, inspection.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Cleaning, cleaning.MaintenanceType);
            Assert.AreEqual(MaintenanceType.Repair, repair.MaintenanceType);
            Assert.AreEqual(MaintenanceType.SafetyCheck, safety.MaintenanceType);
        }

        [TestMethod]
        public void SetPerformedDate_PastDate_Success()
        {
            // Arrange
            var pastDate = DateTime.Now.AddDays(-7);

            // Act
            var record = new MaintenanceRecord
            {
                PerformedDate = pastDate,
                Description = "Test"
            };

            // Assert
            Assert.AreEqual(pastDate.Date, record.PerformedDate.Date);
        }

        [TestMethod]
        public void SetPerformedDate_FutureDate_IsAllowed()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(1);

            // Act
            var record = new MaintenanceRecord
            {
                PerformedDate = futureDate,
                Description = "Test"
            };

            // Assert
            Assert.AreEqual(futureDate.Date, record.PerformedDate.Date);
        }

        [TestMethod]
        public void CreateMaintenanceRecord_WithScheduleLink_Success()
        {
            // Arrange
            var scheduleId = Guid.NewGuid();
            var attractionId = Guid.NewGuid();
            var operatorId = Guid.NewGuid();

            // Act
            var record = new MaintenanceRecord
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

            // Assert
            Assert.IsNotNull(record);
            Assert.AreEqual(scheduleId, record.MaintenanceScheduleId);
            Assert.AreEqual(attractionId, record.AttractionId);
            Assert.AreEqual(operatorId, record.PerformedBy);
            Assert.AreEqual(TimeSpan.FromHours(3), record.Duration);
        }

        [TestMethod]
        public void CreateMaintenanceRecord_UnscheduledMaintenance_Success()
        {
            // Arrange & Act - No schedule ID for emergency maintenance
            var record = new MaintenanceRecord
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

            // Assert
            Assert.IsNotNull(record);
            Assert.IsNull(record.MaintenanceScheduleId);
        }

        [TestMethod]
        public void SetAttractionId_ValidGuid_Success()
        {
            // Arrange
            var attractionId = Guid.NewGuid();

            // Act
            var record = new MaintenanceRecord
            {
                AttractionId = attractionId,
                Description = "Test",
                PerformedDate = DateTime.Now
            };

            // Assert
            Assert.AreEqual(attractionId, record.AttractionId);
        }
    }
}
