using System.Security.Claims;
using CampusFix.Api.Data;
using CampusFix.Api.Models;
using CampusFix.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusFix.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(CampusDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReportResponse>>> List([FromQuery] string? status, [FromQuery] string? q, CancellationToken token)
    {
        status = status?.Trim();
        q = q?.Trim();
        if (!string.IsNullOrEmpty(status) && !ReportOptions.Statuses.Contains(status))
            ModelState.AddModelError(nameof(status), "Choose a valid report status.");
        if (q?.Length > 100) ModelState.AddModelError(nameof(q), "Search must be 100 characters or fewer.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var query = VisibleReports();
        if (!string.IsNullOrEmpty(status)) query = query.Where(x => x.Status == status);
        if (!string.IsNullOrEmpty(q))
        {
            var search = q.ToUpperInvariant();
            query = query.Where(x => x.Title.ToUpper().Contains(search) || x.Location.ToUpper().Contains(search));
        }
        var reports = await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync(token);
        return Ok(reports.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> Get(Guid id, CancellationToken token)
    {
        var report = await VisibleReports().SingleOrDefaultAsync(x => x.Id == id, token);
        return report is null ? NotFound() : Ok(ToResponse(report));
    }

    [HttpPost]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportResponse>> Create(CreateReportRequest request, CancellationToken token)
    {
        var title = request.Title.Trim();
        var location = request.Location.Trim();
        var description = request.Description.Trim();
        var category = request.Category.Trim();
        if (title.Length < 5) ModelState.AddModelError(nameof(request.Title), "Enter a title with at least 5 characters.");
        if (location.Length < 3) ModelState.AddModelError(nameof(request.Location), "Enter a location with at least 3 characters.");
        if (description.Length < 10) ModelState.AddModelError(nameof(request.Description), "Describe the fault in at least 10 characters.");
        if (!ReportOptions.Categories.Contains(category)) ModelState.AddModelError(nameof(request.Category), "Choose a valid category.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var user = await db.Users.FindAsync([CurrentUserId()], token);
        if (user is null) return Unauthorized();
        var report = new FaultReport
        {
            Title = title, Location = location, Category = category, Description = description,
            ReporterId = user.Id, Reporter = user
        };
        db.Reports.Add(report);
        await db.SaveChangesAsync(token);
        return CreatedAtAction(nameof(Get), new { id = report.Id }, ToResponse(report));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Staff")]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> UpdateStatus(Guid id, UpdateStatusRequest request, CancellationToken token)
    {
        var status = request.Status.Trim();
        if (!ReportOptions.Statuses.Contains(status))
        {
            ModelState.AddModelError(nameof(request.Status), "Choose a valid report status.");
            return ValidationProblem(ModelState);
        }
        var report = await db.Reports.Include(x => x.Reporter).SingleOrDefaultAsync(x => x.Id == id, token);
        if (report is null) return NotFound();
        if (report.Status != status)
        {
            report.Status = status;
            report.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(token);
        }
        return Ok(ToResponse(report));
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IQueryable<FaultReport> VisibleReports()
    {
        var query = db.Reports.AsNoTracking().Include(x => x.Reporter).AsQueryable();
        if (!User.IsInRole("Staff"))
        {
            var id = CurrentUserId();
            query = query.Where(x => x.ReporterId == id);
        }
        return query;
    }

    private static ReportResponse ToResponse(FaultReport report) => new(
        report.Id, report.Title, report.Location, report.Category, report.Description, report.Status,
        new DateTimeOffset(DateTime.SpecifyKind(report.CreatedAt, DateTimeKind.Utc)),
        report.UpdatedAt is { } updated ? new DateTimeOffset(DateTime.SpecifyKind(updated, DateTimeKind.Utc)) : null,
        report.ReporterId, report.Reporter.FullName);
}
