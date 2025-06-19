using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AuthApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

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
            var response = await _client.GetAsync("api/auth/current-user");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(json);

            if (jsonDoc.RootElement.TryGetProperty("User", out var userElement))
            {
                return JsonSerializer.Deserialize<UserDto>(userElement.ToString(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }
        catch
        {
            return null;
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

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }
        catch
        {
            return false;
        }
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