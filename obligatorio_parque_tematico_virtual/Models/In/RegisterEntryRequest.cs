using System;
using System.ComponentModel.DataAnnotations;

namespace Models.In
{
    public class RegisterEntryRequest
    {
        [Required]
        public DateTime EnterDate { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}