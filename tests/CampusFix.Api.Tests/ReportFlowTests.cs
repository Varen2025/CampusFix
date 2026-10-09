using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampusFix.Api.Data;
using CampusFix.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CampusFix.Api.Tests;

public sealed class ReportFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string StudentPassword = "Test-student-password!123";

    [Fact]
    public async Task RegistrationCreatesOnlyStudentsAndHashesPassword()
    {
        using var client = factory.CreateClient();
        var email = $"student-{Guid.NewGuid():N}@example.test";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FullName = "  Test Student  ", Email = email, Password = StudentPassword, Role = "Staff"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        Assert.Equal("Student", auth.User.Role);
        Assert.Equal("Test Student", auth.User.FullName);
        Assert.True(auth.ExpiresAt > DateTimeOffset.UtcNow);

        await using var scope = factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<CampusDbContext>().Users.SingleAsync(x => x.Id == auth.User.Id);
        Assert.NotEqual(StudentPassword, user.PasswordHash);
        Assert.NotEmpty(user.PasswordHash);

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Other Student", Email = email.ToUpperInvariant(), Password = StudentPassword
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email.ToUpperInvariant(), Password = StudentPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var incorrectLogin = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = "not-the-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, incorrectLogin.StatusCode);
    }

    [Fact]
    public async Task OwnerCanReadPersistedReportButOtherStudentsCannotReadOrUpdateIt()
    {
        using var owner = await StudentClient();
        using var other = await StudentClient();
        using var anonymous = factory.CreateClient();
        var report = await CreateReport(owner, "Broken projector");
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/reports")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/api/reports", ReportRequest("Another fault"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PatchAsJsonAsync($"/api/reports/{report.Id}/status", new UpdateStatusRequest { Status = "Resolved" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/reports/{report.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await owner.PatchAsJsonAsync($"/api/reports/{report.Id}/status", new UpdateStatusRequest { Status = "Resolved" })).StatusCode);
        var otherReports = (await other.GetFromJsonAsync<List<ReportResponse>>("/api/reports"))!;
        Assert.DoesNotContain(otherReports, x => x.Id == report.Id);

        // A new request scope must retrieve the database record, rather than only an in-memory object.
        var stored = (await owner.GetFromJsonAsync<ReportResponse>($"/api/reports/{report.Id}"))!;
        Assert.Equal("Broken projector", stored.Title);
        Assert.Equal("Open", stored.Status);
        Assert.Equal(report.ReporterId, stored.ReporterId);
        await using var scope = factory.Services.CreateAsyncScope();
        Assert.True(await scope.ServiceProvider.GetRequiredService<CampusDbContext>().Reports.AnyAsync(x => x.Id == report.Id));
    }

    [Fact]
    public async Task StaffCanResolveReportAndStudentCanFilterUpdatedStatus()
    {
        using var student = await StudentClient();
        using var staff = await StaffClient();
        var marker = Guid.NewGuid().ToString("N");
        var report = await CreateReport(student, $"Broken light {marker}");
        var all = (await staff.GetFromJsonAsync<List<ReportResponse>>($"/api/reports?q={marker}"))!;
        Assert.Contains(all, x => x.Id == report.Id);
        var invalid = await staff.PatchAsJsonAsync($"/api/reports/{report.Id}/status", new UpdateStatusRequest { Status = "MadeUp" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var update = await staff.PatchAsJsonAsync($"/api/reports/{report.Id}/status", new UpdateStatusRequest { Status = "Resolved" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = (await update.Content.ReadFromJsonAsync<ReportResponse>())!;
        Assert.Equal("Resolved", updated.Status);
        Assert.NotNull(updated.UpdatedAt);
        var open = (await student.GetFromJsonAsync<List<ReportResponse>>($"/api/reports?status=Open&q={marker}"))!;
        Assert.Empty(open);
        var resolved = (await student.GetFromJsonAsync<List<ReportResponse>>($"/api/reports?status=Resolved&q={marker}"))!;
        Assert.Equal(report.Id, Assert.Single(resolved).Id);
        var missing = await staff.PatchAsJsonAsync($"/api/reports/{Guid.NewGuid()}/status", new UpdateStatusRequest { Status = "Open" });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task InvalidAndWhitespaceInputsAreRejectedWithoutSaving()
    {
        using var student = await StudentClient();
        var request = ReportRequest("     ");
        Assert.Equal(HttpStatusCode.BadRequest, (await student.PostAsJsonAsync("/api/reports", request)).StatusCode);
        request = ReportRequest("Valid title");
        request.Category = "Unexpected category";
        Assert.Equal(HttpStatusCode.BadRequest, (await student.PostAsJsonAsync("/api/reports", request)).StatusCode);
        request.Category = "Electrical";
        request.Description = "          ";
        Assert.Equal(HttpStatusCode.BadRequest, (await student.PostAsJsonAsync("/api/reports", request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await student.GetAsync("/api/reports?status=Invalid")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await student.GetAsync($"/api/reports?q={new string('a', 101)}")).StatusCode);
        Assert.Empty((await student.GetFromJsonAsync<List<ReportResponse>>("/api/reports"))!);
        using var anonymous = factory.CreateClient();
        var invalidRegistration = await anonymous.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "  ", Email = "invalid@example.test", Password = StudentPassword
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidRegistration.StatusCode);
    }

    [Fact]
    public async Task InvalidBearerTokenCannotAccessReportsAndDocumentationIsAvailable()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tampered.token.value");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/reports")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
        var swagger = await client.GetStringAsync("/swagger/v1/swagger.json");
        Assert.Contains("/api/reports", swagger);
        Assert.Contains("/api/auth/login", swagger);
    }

    private async Task<HttpClient> StudentClient()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Student Demo", Email = $"student-{Guid.NewGuid():N}@example.test", Password = StudentPassword
        });
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    private async Task<HttpClient> StaffClient()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = ApiFactory.StaffEmail, Password = ApiFactory.StaffPassword });
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    private static CreateReportRequest ReportRequest(string title) => new()
    {
        Title = title, Location = "Room B12", Category = "Electrical", Description = "The classroom equipment has stopped working."
    };

    private static async Task<ReportResponse> CreateReport(HttpClient client, string title)
    {
        var response = await client.PostAsJsonAsync("/api/reports", ReportRequest(title));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        return (await response.Content.ReadFromJsonAsync<ReportResponse>())!;
    }
}
