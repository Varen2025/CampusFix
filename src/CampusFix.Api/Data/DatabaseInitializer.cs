using System.ComponentModel.DataAnnotations;
using CampusFix.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusDbContext>();
        // Initial coursework schema only. Use real migrations before changing a populated database.
        await db.Database.EnsureCreatedAsync();
        var email = configuration["Seed:StaffEmail"]?.Trim();
        var password = configuration["Seed:StaffPassword"];
        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password)) return;
        if (email is null || email.Length > 254 || !new EmailAddressAttribute().IsValid(email)
            || password is null || password.Length is < 12 or > 128 || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Supply both a valid Seed__StaffEmail and a Seed__StaffPassword of 12–128 characters.");

        var normalizedEmail = email.ToUpperInvariant();
        if (await db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail))
        {
            logger.LogInformation("Staff seed skipped: that email already exists; existing roles and passwords are never changed.");
            return;
        }
        var user = new AppUser { FullName = "Campus maintenance", Email = email, NormalizedEmail = normalizedEmail, Role = "Staff" };
        user.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
