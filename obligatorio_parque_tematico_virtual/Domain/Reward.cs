namespace Domain
{
    public class Reward
    {
        private string _name;
        private string _description;
        private int _pointsCost;
        private int _availableQuantity;

        public Guid Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Name is required");
                }
                if (value.Length > 100)
                {
                    throw new ArgumentException("Name cannot exceed 100 characters");
                }
                _name = value;
            }
        }

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

        public int PointsCost
        {
            get => _pointsCost;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Points cost must be greater than zero");
                }
                _pointsCost = value;
            }
        }

        public int AvailableQuantity
        {
            get => _availableQuantity;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Available quantity cannot be negative");
                }
                _availableQuantity = value;
            }
        }

        public MembershipLevel? RequiredMembershipLevel { get; set; }

        public void DecrementQuantity()
        {
            if (_availableQuantity == 0)
            {
                throw new InvalidOperationException("Cannot decrement quantity when it is already zero");
            }
            _availableQuantity--;
        }

        public bool IsAvailable()
        {
            return _availableQuantity > 0;
        }
    }
}
