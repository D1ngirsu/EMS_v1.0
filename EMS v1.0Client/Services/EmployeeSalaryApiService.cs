using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeSalaryService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeSalaryService(string baseUrl)
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

    public async Task<EmployeeSalaryListResponse> GetAllAsync()
    {
        var response = await _client.GetAsync("api/employee-salary");
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
}

public class EmployeeSalaryDto
{
    public int Eid { get; set; }
    public decimal Salary { get; set; }
    public EmployeeDto Employee { get; set; }
}