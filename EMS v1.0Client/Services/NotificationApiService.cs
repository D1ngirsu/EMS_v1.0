using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class NotificationApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public NotificationApiService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
        _client.BaseAddress = new Uri(baseUrl);
    }

    public async Task<GenericResponse> CreateNotificationAsync(string content)
    {
        try
        {
            var requestBody = new { Content = content };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/notification", httpContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseJson}");

            return JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateNotificationAsync: {ex.Message}");
            return new GenericResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<GenericResponse> GetRecentNotificationsAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/notification/recent");
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseJson}");

            return JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetRecentNotificationsAsync: {ex.Message}");
            return new GenericResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
    }
}