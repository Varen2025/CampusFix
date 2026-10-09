using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusFix.Web.Controllers;

[AllowAnonymous]
public sealed class HomeController : Controller
{
    public IActionResult Index() => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Reports") : View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }
}
