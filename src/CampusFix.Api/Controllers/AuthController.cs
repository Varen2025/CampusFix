using CampusFix.Api.Data;
using CampusFix.Api.Models;
using CampusFix.Api.Services;
using CampusFix.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CampusFix.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("authentication")]
public sealed class AuthController(CampusDbContext db, IPasswordHasher<AppUser> hasher, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken token)
    {
        var fullName = request.FullName.Trim();
        if (fullName.Length < 2) ModelState.AddModelError(nameof(request.FullName), "Enter a name with at least 2 characters.");
        if (string.IsNullOrWhiteSpace(request.Password)) ModelState.AddModelError(nameof(request.Password), "Enter a password of 12–128 characters.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        if (await db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, token))
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Unable to register with these details.");

        var user = new AppUser { FullName = fullName, Email = email, NormalizedEmail = normalizedEmail };
        user.PasswordHash = hasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(token); }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }
            or SqliteException { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 })
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Unable to register with these details.");
        }
        return Ok(tokens.Create(user));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken token)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, token);
        if (user is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email or password is incorrect.");
        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email or password is incorrect.");
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            await db.SaveChangesAsync(token);
        }
        return Ok(tokens.Create(user));
    }
}
