namespace Domain
{
    public class ScoreHistory
    {
        private string _description = string.Empty;
        private string _strategyName = string.Empty;

        public Guid Id { get; set; }
        public Guid VisitorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Points { get; set; }
        public ScoreOrigin Origin { get; set; }
        public Guid? RelatedEntityId { get; set; }

        public string StrategyName
        {
            get => _strategyName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Strategy name is required");
                }

                _strategyName = value;
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

                if (value.Length > 1000)
                {
                    throw new ArgumentException("Description cannot exceed 1000 characters");
                }

                _description = value;
            }
        }

        public virtual User? Visitor { get; set; }

        public ScoreHistory()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}