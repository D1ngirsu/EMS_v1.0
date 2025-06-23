
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EmployeeApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpClientHandler _handler;

    public EmployeeApiService(string baseUrl, HttpClientHandler handler)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }


    public async Task<EmployeeResponse> CreateEmployeeAsync(Employee employee, byte[] imageData = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(employee), Encoding.UTF8, "application/json"), "employee");
        if (imageData != null)
        {
            content.Add(new ByteArrayContent(imageData), "image", $"image_{Guid.NewGuid()}.jpg");
        }

        var response = await _client.PostAsync("api/employee", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeListResponse> GetEmployeesAsync(int page = 1, int pageSize = 10, string? name = null, int? eid = null, int? unitId = null)
    {
        var queryParams = new List<string>();
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");
        if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
        if (eid.HasValue) queryParams.Add($"eid={eid.Value}");
        if (unitId.HasValue) queryParams.Add($"unitId={unitId.Value}");

        var queryString = string.Join("&", queryParams);
        var response = await _client.GetAsync($"api/employee?{queryString}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeResponse> GetEmployeeAsync(int eid)
    {
        try
        {
            var requestUri = new Uri(_client.BaseAddress, $"api/employee/{eid}");
            var cookies = _handler.CookieContainer.GetCookies(requestUri);
            Debug.WriteLine($"[GetEmployeeAsync] Request URL: {requestUri}");
            Debug.WriteLine($"[GetEmployeeAsync] Cookies count: {cookies?.Count ?? 0}");
            foreach (Cookie cookie in cookies ?? new CookieCollection())
            {
                Debug.WriteLine($"[GetEmployeeAsync] Cookie: {cookie.Name} = {cookie.Value}");
            }

            if (cookies == null || cookies.Count == 0)
            {
                Debug.WriteLine("[GetEmployeeAsync] Warning: No cookies found in CookieContainer");
            }

            var response = await _client.GetAsync($"api/employee/{eid}");
            var responseJson = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[GetEmployeeAsync] Response: {responseJson}");

            var employeeResponse = JsonSerializer.Deserialize<EmployeeResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return employeeResponse;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetEmployeeAsync] Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return new EmployeeResponse
            {
                Success = false,
                Message = $"Lỗi khi lấy thông tin nhân viên: {ex.Message}"
            };
        }
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int eid, Employee employee, byte[] imageData = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(employee), Encoding.UTF8, "application/json"), "employee");
        if (imageData != null)
        {
            content.Add(new ByteArrayContent(imageData), "image", $"image_{Guid.NewGuid()}.jpg");
        }

        var response = await _client.PutAsync($"api/employee/{eid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteEmployeeAsync(int eid)
    {
        var response = await _client.DeleteAsync($"api/employee/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}

public class EmployeeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeListResponse
{
    public bool Success { get; set; }
    public List<EmployeeDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class GenericResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}