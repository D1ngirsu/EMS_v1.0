using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class EmployeeApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeApiService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
        _client.BaseAddress = new Uri(baseUrl);
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(Employee employee, byte[] imageData = null, string imageFileName = null)
    {
        using var content = new MultipartFormDataContent();

        // Add employee fields
        content.Add(new StringContent(employee.Name ?? ""), "Name");
        content.Add(new StringContent(employee.DoB.ToString("yyyy-MM-dd")), "DoB");
        content.Add(new StringContent(employee.UnitId.ToString()), "UnitId");
        content.Add(new StringContent(employee.PositionId.ToString()), "PositionId");
        content.Add(new StringContent(employee.Email ?? ""), "Email");
        content.Add(new StringContent(employee.Phone ?? ""), "Phone");
        content.Add(new StringContent(employee.Address ?? ""), "Address");
        content.Add(new StringContent(employee.Gender ?? ""), "Gender");
        content.Add(new StringContent(employee.Experience.ToString() ?? "0"), "Experience");
        content.Add(new StringContent(employee.BankNumber ?? ""), "BankNumber");
        content.Add(new StringContent(employee.Bank ?? ""), "Bank");
        content.Add(new StringContent(employee.Source ?? ""), "Source");

        // Handle image
        if (imageData != null && imageData.Length > 0 && !string.IsNullOrEmpty(imageFileName))
        {
            var imageContent = new ByteArrayContent(imageData);
            var extension = Path.GetExtension(imageFileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "image/jpeg"
            };
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "image", imageFileName);
            Console.WriteLine($"Image added to request, length: {imageData.Length} bytes, fileName: {imageFileName}");
        }
        else
        {
            content.Add(new StringContent(""), "Img");
            Console.WriteLine("No image data provided, Img set to empty");
        }

        try
        {
            var response = await _client.PostAsync("api/employee", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseJson}");

            return JsonSerializer.Deserialize<EmployeeResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateEmployeeAsync: {ex.Message}");
            return new EmployeeResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int eid, Employee employee, byte[] imageData = null, string imageFileName = null)
    {
        using var content = new MultipartFormDataContent();

        // Add employee fields
        content.Add(new StringContent(employee.Eid.ToString()), "Eid");
        content.Add(new StringContent(employee.Name ?? ""), "Name");
        content.Add(new StringContent(employee.DoB.ToString("yyyy-MM-dd")), "DoB");
        content.Add(new StringContent(employee.UnitId.ToString()), "UnitId");
        content.Add(new StringContent(employee.PositionId.ToString()), "PositionId");
        content.Add(new StringContent(employee.Email ?? ""), "Email");
        content.Add(new StringContent(employee.Phone ?? ""), "Phone");
        content.Add(new StringContent(employee.Address ?? ""), "Address");
        content.Add(new StringContent(employee.Gender ?? ""), "Gender");
        content.Add(new StringContent(employee.Experience.ToString() ?? "0"), "Experience");
        content.Add(new StringContent(employee.BankNumber ?? ""), "BankNumber");
        content.Add(new StringContent(employee.Bank ?? ""), "Bank");
        content.Add(new StringContent(employee.Source ?? ""), "Source");

        // Handle image
        if (imageData != null && imageData.Length > 0 && !string.IsNullOrEmpty(imageFileName))
        {
            var imageContent = new ByteArrayContent(imageData);
            var extension = Path.GetExtension(imageFileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "image/jpeg"
            };
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "image", imageFileName);
            Console.WriteLine($"Image added to request, length: {imageData.Length} bytes, fileName: {imageFileName}");
        }
        else
        {
            content.Add(new StringContent(employee.Img ?? ""), "Img");
            Console.WriteLine($"No new image data provided, Img set to: {employee.Img ?? "null"}");
        }

        try
        {
            var response = await _client.PutAsync($"api/employee/{eid}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseJson}");

            return JsonSerializer.Deserialize<EmployeeResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateEmployeeAsync: {ex.Message}");
            return new EmployeeResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
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

    public async Task<ApiResponse<EmployeeDto>> GetEmployeeAsync(int eid)
    {
        try
        {
            // Log cookies before request
            var cookies = _httpClientFactory.CookieContainer.GetCookies(_client.BaseAddress);
            Debug.WriteLine($"[GetEmployeeAsync] Cookies count: {cookies.Count}");
            if (cookies.Count == 0)
            {
                Debug.WriteLine("[GetEmployeeAsync] Warning: No cookies found in CookieContainer");
            }
            foreach (System.Net.Cookie cookie in cookies)
            {
                Debug.WriteLine($"[GetEmployeeAsync] Cookie: {cookie.Name} = {cookie.Value}");
            }

            Debug.WriteLine($"[GetEmployeeAsync] Request URL: {_client.BaseAddress}api/employee/{eid}");
            var response = await _client.GetAsync($"api/employee/{eid}");
            var responseJson = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[GetEmployeeAsync] Response: {responseJson}");

            return JsonSerializer.Deserialize<ApiResponse<EmployeeDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetEmployeeAsync] Exception: {ex.Message}");
            return new ApiResponse<EmployeeDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
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
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
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
    public string Message { get; internal set; }
}

public class GenericResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
}