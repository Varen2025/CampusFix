using CampusFix.Contracts;

namespace CampusFix.Web.Models;

public sealed record ReportListViewModel(IReadOnlyList<ReportResponse> Reports, string? Status, string? Query);
