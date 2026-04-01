namespace YouthRetreatRegistration.Models;

public class Registration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string AgeRange { get; set; } = string.Empty;
    public bool IsFromOtherBranch { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Expectations { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
