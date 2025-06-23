using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AuthApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    // Thêm property để expose CookieContainer
    public CookieContainer CookieContainer => _cookieContainer;

    public AuthApiService(string baseUrl)
    {
        _cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler()
        {
            CookieContainer = _cookieContainer,
            UseCookies = true
        };

        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    // Thêm method để tạo HttpClientHandler với cùng CookieContainer
    public HttpClientHandler CreateSharedHandler()
    {
        return new HttpClientHandler()
        {
            CookieContainer = _cookieContainer,
            UseCookies = true
        };
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/auth/login", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<LoginResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await _client.PostAsync("api/auth/logout", null);

            // Clear cookies
            foreach (Cookie cookie in _cookieContainer.GetCookies(_client.BaseAddress))
            {
                cookie.Expired = true;
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDto> GetCurrentUserAsync()
    {
        try
        {
            // Debug: Kiểm tra cookies
            var cookies = _cookieContainer.GetCookies(_client.BaseAddress);
            Debug.WriteLine($"[GetCurrentUser] Cookies count: {cookies.Count}");

            foreach (Cookie cookie in cookies)
            {
                Debug.WriteLine($"[GetCurrentUser] Cookie: {cookie.Name} = {cookie.Value}");
            }

            var response = await _client.GetAsync("api/auth/current-user");
            var responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"[GetCurrentUser] Status: {response.StatusCode}");
            Debug.WriteLine($"[GetCurrentUser] Full Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[GetCurrentUser] Request failed with status: {response.StatusCode}");
                return null;
            }

            // Parse JSON response
            var jsonDoc = JsonDocument.Parse(responseContent);

            // Debug: Check if Success property exists
            if (jsonDoc.RootElement.TryGetProperty("Success", out var successElement))
            {
                bool success = successElement.GetBoolean();
                Debug.WriteLine($"[GetCurrentUser] API Success: {success}");

                if (!success)
                {
                    if (jsonDoc.RootElement.TryGetProperty("Message", out var messageElement))
                    {
                        Debug.WriteLine($"[GetCurrentUser] API Error Message: {messageElement.GetString()}");
                    }
                    return null;
                }
            }
            else if (jsonDoc.RootElement.TryGetProperty("success", out var successElementLower))
            {
                bool success = successElementLower.GetBoolean();
                Debug.WriteLine($"[GetCurrentUser] API success (lowercase): {success}");

                if (!success)
                {
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageElement))
                    {
                        Debug.WriteLine($"[GetCurrentUser] API error message: {messageElement.GetString()}");
                    }
                    return null;
                }
            }

            // Try to get User property (case insensitive)
            JsonElement userElement;
            bool hasUser = jsonDoc.RootElement.TryGetProperty("User", out userElement) ||
                          jsonDoc.RootElement.TryGetProperty("user", out userElement);

            if (hasUser)
            {
                Debug.WriteLine($"[GetCurrentUser] User element found: {userElement.ToString()}");

                var userDto = JsonSerializer.Deserialize<UserDto>(userElement.ToString(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Debug.WriteLine($"[GetCurrentUser] Deserialized UserDto: Username={userDto?.Username}, Role={userDto?.Role}");
                return userDto;
            }
            else
            {
                Debug.WriteLine("[GetCurrentUser] No User property found in response");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetCurrentUser] Exception: {ex.Message}");
            Debug.WriteLine($"[GetCurrentUser] StackTrace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            Debug.WriteLine("[IsAuthenticatedAsync] Checking authentication...");
            var user = await GetCurrentUserAsync();
            bool isAuth = user != null;
            Debug.WriteLine($"[IsAuthenticatedAsync] Result: {isAuth}");
            return isAuth;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[IsAuthenticatedAsync] Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<List<OrganizationUnitDto>> GetOrganizationUnitsAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/auth/organization-units");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(json);
            if (jsonDoc.RootElement.TryGetProperty("Data", out var dataElement))
            {
                return JsonSerializer.Deserialize<List<OrganizationUnitDto>>(dataElement.ToString(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return new List<OrganizationUnitDto>();
        }
        catch
        {
            return new List<OrganizationUnitDto>();
        }
    }

    public async Task<List<PositionDto>> GetPositionsAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/auth/positions");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(json);
            if (jsonDoc.RootElement.TryGetProperty("Data", out var dataElement))
            {
                return JsonSerializer.Deserialize<List<PositionDto>>(dataElement.ToString(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return new List<PositionDto>();
        }
        catch
        {
            return new List<PositionDto>();
        }
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var request = new ChangePasswordRequest { CurrentPassword = currentPassword, NewPassword = newPassword };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/auth/change-password", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ChangePasswordResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}

public class ChangePasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}