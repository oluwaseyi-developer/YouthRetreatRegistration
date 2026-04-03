using Microsoft.EntityFrameworkCore;
using YouthRetreatRegistration.Models;

namespace YouthRetreatRegistration.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.AgeRange).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BranchName).HasMaxLength(100);
            entity.Property(e => e.Expectations).HasMaxLength(500);
            entity.Property(e => e.Gender).HasConversion<string>().HasMaxLength(10);
            entity.Property(e => e.HasAttended).HasDefaultValue(false);

            entity.HasIndex(e => new { e.FullName, e.PhoneNumber });
        });
    }
}
