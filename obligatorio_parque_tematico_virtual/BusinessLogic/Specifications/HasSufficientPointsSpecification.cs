using Domain;

namespace BusinessLogic.Specifications
{
    public class HasSufficientPointsSpecification : ISpecification<User>
    {
        private readonly int _requiredPoints;

        public HasSufficientPointsSpecification(int requiredPoints)
        {
            _requiredPoints = requiredPoints;
        }

        public bool IsSatisfiedBy(User candidate)
        {
            return candidate.Score >= _requiredPoints;
        }
    }
}
