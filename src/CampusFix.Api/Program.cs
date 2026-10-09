using System.Text;
using System.Threading.RateLimiting;
using CampusFix.Api.Data;
using CampusFix.Api.Models;
using CampusFix.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("Set Jwt__Key to a random secret with at least 32 characters.");

var provider = builder.Configuration["DbProvider"] ?? (builder.Environment.IsDevelopment() ? "Sqlite" : "Postgres");
var connection = builder.Configuration.GetConnectionString("CampusFix");
if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
        throw new InvalidOperationException("SQLite is available only in Development or Testing. Use PostgreSQL for production.");
    if (string.IsNullOrWhiteSpace(connection))
    {
        var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        connection = $"Data Source={Path.Combine(dataDirectory, "campusfix.db")}";
    }
    builder.Services.AddDbContext<CampusDbContext>(options => options.UseSqlite(connection));
}
else if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    if (string.IsNullOrWhiteSpace(connection))
        throw new InvalidOperationException("Set ConnectionStrings__CampusFix to your PostgreSQL connection string.");
    builder.Services.AddDbContext<CampusDbContext>(options => options.UseNpgsql(connection));
}
else
    throw new InvalidOperationException("DbProvider must be Sqlite or Postgres.");

builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "CampusFix",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "CampusFix",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
            AutoReplenishment = true
        }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CampusFix API",
        Version = "v1",
        Description = "Register or log in to receive a bearer token. Students see their own reports; staff can see and update all reports."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the token returned by /api/auth/login."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();
// Keep exception details out of responses in every environment.
app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    await next();
});
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", async (CampusDbContext db, CancellationToken token) =>
    await db.Database.CanConnectAsync(token)
        ? Results.Ok(new { status = "healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration, app.Logger);
app.Run();

public partial class Program;
