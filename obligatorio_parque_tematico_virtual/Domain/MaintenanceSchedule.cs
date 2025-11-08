namespace Domain
{
    public class MaintenanceSchedule
    {
        private string _description = string.Empty;

        public Guid Id { get; set; }
        public Guid AttractionId { get; set; }
        public DateTime ScheduledDate { get; set; }

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

        public MaintenanceStatus Status { get; set; }

        public virtual Attraction? Attraction { get; set; }

        public MaintenanceSchedule()
        {
            Status = MaintenanceStatus.Pending;
        }

        public bool IsOverdue(DateTime currentDateTime)
        {
            if (Status == MaintenanceStatus.Completed || Status == MaintenanceStatus.Cancelled)
            {
                return false;
            }

            return ScheduledDate < currentDateTime;
        }

        public bool CanComplete()
        {
            return Status == MaintenanceStatus.Pending || Status == MaintenanceStatus.InProgress;
        }
    }
}