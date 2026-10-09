using CampusFix.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Api.Data;

public sealed class CampusDbContext(DbContextOptions<CampusDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<FaultReport> Reports => Set<FaultReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<AppUser>();
        user.HasKey(x => x.Id);
        user.HasIndex(x => x.NormalizedEmail).IsUnique();
        user.Property(x => x.FullName).HasMaxLength(80).IsRequired();
        user.Property(x => x.Email).HasMaxLength(254).IsRequired();
        user.Property(x => x.NormalizedEmail).HasMaxLength(254).IsRequired();
        user.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        user.Property(x => x.Role).HasMaxLength(20).IsRequired();

        var report = modelBuilder.Entity<FaultReport>();
        report.HasKey(x => x.Id);
        report.Property(x => x.Title).HasMaxLength(100).IsRequired();
        report.Property(x => x.Location).HasMaxLength(120).IsRequired();
        report.Property(x => x.Category).HasMaxLength(30).IsRequired();
        report.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        report.Property(x => x.Status).HasMaxLength(20).IsRequired();
        report.HasOne(x => x.Reporter).WithMany().HasForeignKey(x => x.ReporterId).OnDelete(DeleteBehavior.Restrict);
        report.HasIndex(x => new { x.ReporterId, x.CreatedAt });
        report.HasIndex(x => x.Status);
    }
}
