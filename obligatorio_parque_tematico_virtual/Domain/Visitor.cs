using System;

namespace Domain
{
    public class Visitor : User
    {
        public Visitor()
        {
            MembershipLevel = Domain.MembershipLevel.Standard;
        }
    }
}