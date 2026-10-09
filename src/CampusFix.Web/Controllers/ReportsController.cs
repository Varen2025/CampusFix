using System.Net;
using CampusFix.Contracts;
using CampusFix.Web.Models;
using CampusFix.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusFix.Web.Controllers;

[Authorize]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class ReportsController(CampusApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? status, string? q, CancellationToken ct)
    {
        status = ReportOptions.Statuses.Contains(status) ? status : null;
        q = q?.Trim();
        if (q?.Length > 100) q = q[..100];
        return View(new ReportListViewModel(await api.GetReportsAsync(status, q, ct), status, q));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct) => View(await api.GetReportAsync(id, ct));

    [Authorize(Roles = "Student"), HttpGet]
    public IActionResult Create() => View(new CreateReportRequest());

    [Authorize(Roles = "Student"), HttpPost]
    public async Task<IActionResult> Create(CreateReportRequest input, CancellationToken ct)
    {
        if (!ReportOptions.Categories.Contains(input.Category)) ModelState.AddModelError(nameof(input.Category), "Choose a category from the list.");
        if (!ModelState.IsValid) return View(input);
        try
        {
            var report = await api.CreateReportAsync(input, ct);
            TempData["Success"] = "Report submitted. You can follow its progress here.";
            return RedirectToAction(nameof(Details), new { id = report.Id });
        }
        catch (ApiException ex) when (ex.StatusCode != HttpStatusCode.Unauthorized)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(input);
        }
    }

    [Authorize(Roles = "Staff"), HttpPost]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusRequest input, CancellationToken ct)
    {
        if (!ReportOptions.Statuses.Contains(input.Status)) ModelState.AddModelError(nameof(input.Status), "Choose a valid status.");
        if (!ModelState.IsValid) return View("Details", await api.GetReportAsync(id, ct));
        try
        {
            await api.UpdateStatusAsync(id, input, ct);
            TempData["Success"] = "Report status updated. The student can now see the change.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ApiException ex) when (ex.StatusCode != HttpStatusCode.Unauthorized)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Details", await api.GetReportAsync(id, ct));
        }
    }
}
