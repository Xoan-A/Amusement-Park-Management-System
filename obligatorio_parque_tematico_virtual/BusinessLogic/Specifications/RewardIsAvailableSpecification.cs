using Domain;

namespace BusinessLogic.Specifications
{
    public class RewardIsAvailableSpecification : ISpecification<Reward>
    {
        public bool IsSatisfiedBy(Reward candidate)
        {
            return candidate.IsAvailable();
        }
    }
}
