using Domain;

namespace BusinessLogic.Specifications
{
    public class MeetsRequiredMembershipSpecification : ISpecification<User>
    {
        private readonly MembershipLevel? _requiredLevel;

        public MeetsRequiredMembershipSpecification(MembershipLevel? requiredLevel)
        {
            _requiredLevel = requiredLevel;
        }

        public bool IsSatisfiedBy(User candidate)
        {
            if (_requiredLevel == null)
            {
                return true;
            }

            if (candidate.MembershipLevel == null)
            {
                return false;
            }

            return candidate.MembershipLevel >= _requiredLevel;
        }
    }
}
