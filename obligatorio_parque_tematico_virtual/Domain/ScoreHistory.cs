namespace Domain
{
    public class ScoreHistory
    {
        private string _strategyName = string.Empty;

        public Guid Id { get; set; }
        public Guid VisitorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Points { get; set; }
        public ScoreOrigin Origin { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityName { get; set; }

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

        public virtual User? Visitor { get; set; }
    }
}