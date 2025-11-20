using System.ComponentModel.DataAnnotations;

namespace Models.In
{
    public class RegisterEntryRequest
    {
        public Guid? Qr { get; set; }
        public Guid? NFC { get; set; }
        public Guid? EventId { get; set; }
    }
}