using System.Net;
using System.Security.Claims;
using CampusFix.Contracts;
using CampusFix.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusFix.Web.Controllers;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AccountController(CampusApiClient api) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl, bool sessionExpired = false)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Reports");
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        ViewData["SessionExpired"] = sessionExpired;
        return View(new LoginRequest());
    }

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> Login(LoginRequest input, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (!ModelState.IsValid) return View(input);
        try
        {
            var response = await api.LoginAsync(input, ct);
            await SignInAsync(response);
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Reports");
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(input);
        }
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Register() => User.Identity?.IsAuthenticated == true
        ? RedirectToAction("Index", "Reports") : View(new RegisterRequest());

    [AllowAnonymous, HttpPost]
    public async Task<IActionResult> Register(RegisterRequest input, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(input);
        try
        {
            var response = await api.RegisterAsync(input, ct);
            await SignInAsync(response);
            TempData["Success"] = "Your student account is ready. Welcome to CampusFix.";
            return RedirectToAction("Index", "Reports");
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(input);
        }
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private Task SignInAsync(AuthResponse response)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
            new Claim(ClaimTypes.Name, response.User.FullName),
            new Claim(ClaimTypes.Role, response.User.Role),
            new Claim("access_token", response.Token)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties
            {
                ExpiresUtc = response.ExpiresAt,
                IsPersistent = false,
                AllowRefresh = false
            });
    }
}
