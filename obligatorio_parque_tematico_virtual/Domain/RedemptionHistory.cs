namespace Domain
{
    public class RedemptionHistory
    {
        private Guid _visitorId;
        private Guid _rewardId;
        private int _pointsSpent;
        private DateTime _redeemedAt;

        public Guid Id { get; set; }

        public Guid VisitorId
        {
            get => _visitorId;
            set
            {
                if (value == Guid.Empty)
                {
                    throw new ArgumentException("Visitor ID cannot be empty");
                }
                _visitorId = value;
            }
        }

        public Guid RewardId
        {
            get => _rewardId;
            set
            {
                if (value == Guid.Empty)
                {
                    throw new ArgumentException("Reward ID cannot be empty");
                }
                _rewardId = value;
            }
        }

        public DateTime RedeemedAt
        {
            get => _redeemedAt;
            set => _redeemedAt = value;
        }

        public int PointsSpent
        {
            get => _pointsSpent;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Points spent must be greater than zero");
                }
                _pointsSpent = value;
            }
        }

        public User? Visitor { get; set; }
        public Reward? Reward { get; set; }
    }
}