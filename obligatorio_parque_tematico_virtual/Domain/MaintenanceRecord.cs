namespace Domain
{
    public class MaintenanceRecord
    {
        private string _description = string.Empty;
        private string? _notes;
        private TimeSpan _duration;

        public Guid Id { get; set; }
        public Guid? MaintenanceScheduleId { get; set; }
        public Guid AttractionId { get; set; }
        public DateTime PerformedDate { get; set; }
        public Guid PerformedBy { get; set; }

        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Description is required");
                }

                if (value.Length > 500)
                {
                    throw new ArgumentException("Description cannot exceed 500 characters");
                }

                _description = value;
            }
        }

        public string? Notes
        {
            get => _notes;
            set
            {
                if (value != null && value.Length > 1000)
                {
                    throw new ArgumentException("Notes cannot exceed 1000 characters");
                }

                _notes = value;
            }
        }

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException("Duration cannot be negative");
                }

                _duration = value;
            }
        }

        public DateTime CreatedAt { get; set; }

        public virtual MaintenanceSchedule? MaintenanceSchedule { get; set; }
        public virtual Attraction? Attraction { get; set; }
        public virtual User? Operator { get; set; }
    }
}