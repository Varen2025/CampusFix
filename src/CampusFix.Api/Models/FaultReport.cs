namespace CampusFix.Api.Models;

public sealed class FaultReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid ReporterId { get; set; }
    public AppUser Reporter { get; set; } = null!;
}
