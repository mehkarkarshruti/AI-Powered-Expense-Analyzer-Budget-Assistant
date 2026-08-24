using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpenseAnalyzer.Web.Services;

public class ApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public ApiService(
        IHttpClientFactory factory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _factory = factory;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();

        var baseUrl = _configuration["ApiSettings:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "API BaseUrl is not configured.");
        }

        client.BaseAddress = new Uri(baseUrl);

        var token = _httpContextAccessor.HttpContext?
            .Session.GetString("JwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var client = CreateClient();

        return await client.GetFromJsonAsync<T>(endpoint);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data)
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(endpoint, data);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<TResponse>();
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        var client = CreateClient();

        var response = await client.DeleteAsync(endpoint);

        return response.IsSuccessStatusCode;
    }
}
