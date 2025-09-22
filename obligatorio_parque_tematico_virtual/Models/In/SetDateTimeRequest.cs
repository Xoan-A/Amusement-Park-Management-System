using System.ComponentModel.DataAnnotations;

namespace Models.In
{
    public class SetDateTimeRequest
    {
        [Required]
        public string DateTime { get; set; }
    }
}