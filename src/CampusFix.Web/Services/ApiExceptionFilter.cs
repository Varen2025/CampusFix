using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CampusFix.Web.Services;

public sealed class ApiExceptionFilter(IModelMetadataProvider metadataProvider) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ApiException exception) return;
        context.ExceptionHandled = true;
        if (exception.StatusCode == HttpStatusCode.Unauthorized && context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Result = new RedirectToActionResult("Login", "Account", new { sessionExpired = true });
            return;
        }

        context.Result = new ViewResult
        {
            ViewName = "~/Views/Shared/ServiceError.cshtml",
            StatusCode = (int)exception.StatusCode,
            ViewData = new ViewDataDictionary(metadataProvider, context.ModelState)
            {
                ["Title"] = "Something needs attention",
                ["Message"] = exception.Message
            }
        };
    }
}
