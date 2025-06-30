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
    }

    public async Task<EmployeeSalaryListResponse> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-salary?page={page}&pageSize={pageSize}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeSalaryListResponse>(responseJson, new JsonSerializerOptions
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
    public List<EmployeeSalaryDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
