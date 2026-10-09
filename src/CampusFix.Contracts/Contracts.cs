using System.ComponentModel.DataAnnotations;

namespace CampusFix.Contracts;

public sealed class RegisterRequest
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = "";
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128)]
    public string Password { get; set; } = "";
}

public sealed class CreateReportRequest
{
    [Required, StringLength(100, MinimumLength = 5)]
    public string Title { get; set; } = "";
    [Required, StringLength(120, MinimumLength = 3)]
    public string Location { get; set; } = "";
    [Required]
    public string Category { get; set; } = "Electrical";
    [Required, StringLength(2000, MinimumLength = 10)]
    public string Description { get; set; } = "";
}

public sealed class UpdateStatusRequest
{
    [Required]
    public string Status { get; set; } = "Open";
}

public sealed record UserSummary(Guid Id, string FullName, string Email, string Role);
public sealed record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserSummary User);
public sealed record ReportResponse(Guid Id, string Title, string Location, string Category,
    string Description, string Status, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    Guid ReporterId, string ReporterName);

public static class ReportOptions
{
    public static readonly string[] Categories = ["Electrical", "Plumbing", "Furniture", "IT equipment", "Other"];
    public static readonly string[] Statuses = ["Open", "InProgress", "Resolved"];
    public static string StatusLabel(string status) => status == "InProgress" ? "In progress" : status;
}
