using System.ComponentModel.DataAnnotations;

namespace YouthRetreatRegistration.Models;

public class RegistrationFormModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your gender.")]
    public Gender? Gender { get; set; }

    [Required(ErrorMessage = "Please select your age range.")]
    public string AgeRange { get; set; } = string.Empty;

    public bool IsFromOtherBranch { get; set; }

    public string BranchName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Expectations must be 500 characters or less.")]
    public string Expectations { get; set; } = string.Empty;
}
