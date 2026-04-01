using Microsoft.EntityFrameworkCore;
using YouthRetreatRegistration.Data;
using YouthRetreatRegistration.Models;

namespace YouthRetreatRegistration.Services;

public sealed class RegistrationService : IRegistrationService
{
    private readonly AppDbContext _db;

    public RegistrationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Registration> AddRegistrationAsync(RegistrationFormModel form)
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            FullName = form.FullName.Trim(),
            PhoneNumber = form.PhoneNumber.Trim(),
            Email = form.Email?.Trim() ?? string.Empty,
            Gender = form.Gender!.Value,
            AgeRange = form.AgeRange,
            IsFromOtherBranch = form.IsFromOtherBranch,
            BranchName = form.IsFromOtherBranch && !string.IsNullOrWhiteSpace(form.BranchName)
                ? form.BranchName.Trim()
                : "Ikotun-Egbe",
            Expectations = form.Expectations?.Trim() ?? string.Empty,
            RegisteredAt = DateTime.UtcNow
        };

        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync();
        return registration;
    }

    public async Task<bool> IsDuplicateAsync(string fullName, string phoneNumber)
    {
        var trimmedName = fullName.Trim();
        var trimmedPhone = phoneNumber.Trim();

        return await _db.Registrations.AnyAsync(r =>
            r.FullName.ToLower() == trimmedName.ToLower() &&
            r.PhoneNumber.ToLower() == trimmedPhone.ToLower());
    }

    public async Task<List<Registration>> GetAllRegistrationsAsync()
    {
        return await _db.Registrations
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();
    }

    public async Task<(List<Registration> Items, int TotalCount)> GetRegistrationsPagedAsync(int page, int pageSize)
    {
        var totalCount = await _db.Registrations.CountAsync();

        var items = await _db.Registrations
            .OrderByDescending(r => r.RegisteredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> DeleteRegistrationAsync(Guid id)
    {
        var registration = await _db.Registrations.FindAsync(id);
        if (registration is null) return false;

        _db.Registrations.Remove(registration);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var all = await _db.Registrations.ToListAsync();

        return new DashboardStats
        {
            TotalRegistrations = all.Count,
            MaleCount = all.Count(r => r.Gender == Gender.Male),
            FemaleCount = all.Count(r => r.Gender == Gender.Female),
            OtherBranchCount = all.Count(r => r.IsFromOtherBranch),
            AgeRangeBreakdown = all
                .GroupBy(r => r.AgeRange)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
