
using System.ComponentModel.DataAnnotations;

namespace SmartClinic.API.Models;

public class PatientCreateRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[LP]$",
        ErrorMessage = "Gender harus L atau P.")]
    public string Gender { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }
}
