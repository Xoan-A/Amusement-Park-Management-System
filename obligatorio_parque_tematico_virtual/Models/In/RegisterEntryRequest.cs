using System;
using System.ComponentModel.DataAnnotations;
using Domain;

namespace Models.In
{
    public class RegisterEntryRequest
    {
        [Required]
        public DateTime EnterDate { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public Guid? Qr { get; set; }
        public Guid? NFC { get; set; }
        public int? EventId { get; set; }
    }
}