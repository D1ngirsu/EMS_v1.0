using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeSalaryService : IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpClientHandler _handler;

    public EmployeeSalaryService(string baseUrl, HttpClientHandler handler)
    {
        if (string.IsNullOrEmpty(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl));
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
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
        _client?.Dispose();
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

public class EmployeeSalaryDto
{
    public int Eid { get; set; }
    public decimal Salary { get; set; }
    public EmployeeDto Employee { get; set; }
}