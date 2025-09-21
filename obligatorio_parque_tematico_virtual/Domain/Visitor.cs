using System;

namespace Domain
{
    public class Visitor : User
    {
        public DateTime BirthDate { get; set; }
        public MembershipLevel MembershipLevel { get; set; }

        public Visitor()
        {
            MembershipLevel = MembershipLevel.Standard;
        }
    }
}