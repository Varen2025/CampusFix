using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CampusFix.Contracts;

namespace CampusFix.Web.Services;

public sealed class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public sealed class CampusApiClient(HttpClient client, IHttpContextAccessor contextAccessor)
{
    public Task<AuthResponse> LoginAsync(LoginRequest input, CancellationToken ct) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", input, ct);

    public Task<AuthResponse> RegisterAsync(RegisterRequest input, CancellationToken ct) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", input, ct);

    public Task<List<ReportResponse>> GetReportsAsync(string? status, string? q, CancellationToken ct)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) query.Add("status=" + Uri.EscapeDataString(status));
        if (!string.IsNullOrWhiteSpace(q)) query.Add("q=" + Uri.EscapeDataString(q));
        return SendAsync<List<ReportResponse>>(HttpMethod.Get, "api/reports" + (query.Count > 0 ? "?" + string.Join('&', query) : ""), null, ct);
    }

    public Task<ReportResponse> GetReportAsync(Guid id, CancellationToken ct) =>
        SendAsync<ReportResponse>(HttpMethod.Get, $"api/reports/{id}", null, ct);

    public Task<ReportResponse> CreateReportAsync(CreateReportRequest input, CancellationToken ct) =>
        SendAsync<ReportResponse>(HttpMethod.Post, "api/reports", input, ct);

    public Task<ReportResponse> UpdateStatusAsync(Guid id, UpdateStatusRequest input, CancellationToken ct) =>
        SendAsync<ReportResponse>(HttpMethod.Patch, $"api/reports/{id}/status", input, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        var token = contextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null) request.Content = JsonContent.Create(body);

        try
        {
            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var message = response.StatusCode switch
                {
                    HttpStatusCode.BadRequest => "Please check the information you entered and try again.",
                    HttpStatusCode.Unauthorized => "The email address or password is incorrect. Please try again.",
                    HttpStatusCode.Forbidden => "You do not have access to this action.",
                    HttpStatusCode.NotFound => "We could not find that report, or it is not available to your account.",
                    HttpStatusCode.Conflict => "That email address is already registered. Please sign in instead.",
                    HttpStatusCode.TooManyRequests => "There have been too many attempts. Please wait a moment and try again.",
                    _ => "CampusFix could not complete your request. Please try again shortly."
                };
                throw new ApiException(response.StatusCode, message);
            }

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
                ?? throw new ApiException(HttpStatusCode.BadGateway, "CampusFix received an incomplete response. Please try again shortly.");
        }
        catch (HttpRequestException)
        {
            throw new ApiException(HttpStatusCode.ServiceUnavailable, "The reporting service is temporarily unavailable. Please try again shortly.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiException(HttpStatusCode.GatewayTimeout, "The reporting service is taking longer than expected. Please try again shortly.");
        }
        catch (JsonException)
        {
            throw new ApiException(HttpStatusCode.BadGateway, "CampusFix received an unexpected response. Please try again shortly.");
        }
    }
}
