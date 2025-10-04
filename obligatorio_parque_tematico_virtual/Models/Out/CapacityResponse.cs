using System.ComponentModel.DataAnnotations;

namespace Models.Out;

public class CapacityResponse
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public int Capacity { get; set; }
    [Required]
    public int CurrentCapacity { get; set; }
}