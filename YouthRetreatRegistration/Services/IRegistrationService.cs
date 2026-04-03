using YouthRetreatRegistration.Models;

namespace YouthRetreatRegistration.Services;

public interface IRegistrationService
{
    Task<Registration> AddRegistrationAsync(RegistrationFormModel form);
    Task<bool> IsDuplicateAsync(string fullName, string phoneNumber);
    Task<List<Registration>> GetAllRegistrationsAsync();
    Task<(List<Registration> Items, int TotalCount)> GetRegistrationsPagedAsync(int page, int pageSize, string? search = null);
    Task<bool> DeleteRegistrationAsync(Guid id);
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<bool> ToggleAttendanceAsync(Guid id);
}
