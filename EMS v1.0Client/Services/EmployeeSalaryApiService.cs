using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeSalaryService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeSalaryService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
        _client.BaseAddress = new Uri(baseUrl);
    }

    public async Task<EmployeeSalaryListResponse> GetAllAsync(int page = 1, int pageSize = 10, string? searchName = null, string? unitName = null, string? sortOrder = null)
    {
        var queryString = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchName))
        {
            queryString += $"&searchName={Uri.EscapeDataString(searchName)}";
        }
        if (!string.IsNullOrEmpty(unitName))
        {
            queryString += $"&unitName={Uri.EscapeDataString(unitName)}";
        }
        if (!string.IsNullOrEmpty(sortOrder))
        {
            queryString += $"&sortOrder={Uri.EscapeDataString(sortOrder)}";
        }

        var response = await _client.GetAsync($"api/employee-salary{queryString}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeSalaryListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    // Thêm method để lấy danh sách phòng ban
    public async Task<DepartmentListResponse> GetDepartmentsAsync()
    {
        var response = await _client.GetAsync("api/employee-salary/departments");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<DepartmentListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeSalaryResponse> GetByEidAsync(int eid)
    {
        var response = await _client.GetAsync($"api/employee-salary/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeSalaryResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeSalaryResponse> CreateAsync(EmployeeSalaryDto salary)
    {
        var json = JsonSerializer.Serialize(salary);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-salary", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeSalaryResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeSalaryResponse> UpdateAsync(int eid, EmployeeSalaryDto salary)
    {
        var json = JsonSerializer.Serialize(salary);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-salary/{eid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeSalaryResponse>(responseJson, new JsonSerializerOptions
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

public class EmployeeSalaryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeSalaryDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeSalaryListResponse
{
    public bool Success { get; set; }
    public List<EmployeeSalaryListDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

// Thêm response class cho danh sách phòng ban
public class DepartmentListResponse
{
    public bool Success { get; set; }
    public List<string> Data { get; set; }
}

public class EmployeeSalaryListDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string PositionName { get; set; }
    public string UnitName { get; set; }
    public decimal? Salary { get; set; }
}