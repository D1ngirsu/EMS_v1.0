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
        
    }

    public async Task<GenericResponse> CreateNotificationAsync(string content)
    {
        try
        {
            Console.WriteLine("[NotificationApiService] Creating notification...");

            var requestBody = new { Content = content };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            LogCurrentCookies();

            var response = await _client.PostAsync("api/notification", httpContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[NotificationApiService] Response Status: {response.StatusCode}");
            Console.WriteLine($"[NotificationApiService] Response Content: {responseJson}");

            if (!response.IsSuccessStatusCode)
            {
                return new GenericResponse
                {
                    Success = false,
                    Message = $"HTTP {response.StatusCode}: {responseJson}"
                };
            }

            return JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error in CreateNotificationAsync: {ex.Message}");
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
            Console.WriteLine("[NotificationApiService] Getting recent notifications...");

            LogCurrentCookies();

            var response = await _client.GetAsync("api/notification/recent");
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[NotificationApiService] Response Status: {response.StatusCode}");
            Console.WriteLine($"[NotificationApiService] Response Content: {responseJson}");

            if (!response.IsSuccessStatusCode)
            {
                return new GenericResponse
                {
                    Success = false,
                    Message = $"HTTP {response.StatusCode}: {responseJson}",
                    Data = new List<object>()
                };
            }

            var result = JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Data == null)
            {
                if (result == null)
                {
                    result = new GenericResponse { Success = false, Message = "Null response" };
                }
                result.Data = new List<object>();
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error in GetRecentNotificationsAsync: {ex.Message}");
            return new GenericResponse
            {
                Success = false,
                Message = ex.Message,
                Data = new List<object>()
            };
        }
    }

    private void LogCurrentCookies()
    {
        try
        {
            var cookies = _httpClientFactory.CookieContainer.GetCookies(_client.BaseAddress);
            Console.WriteLine($"[NotificationApiService] Sending {cookies.Count} cookies:");
            foreach (System.Net.Cookie cookie in cookies)
            {
                Console.WriteLine($"[NotificationApiService] Cookie: {cookie.Name}={cookie.Value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationApiService] Error logging cookies: {ex.Message}");
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