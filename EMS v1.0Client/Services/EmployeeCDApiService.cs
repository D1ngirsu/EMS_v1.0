using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeCDService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeCDService(string baseUrl, HttpClientHandler handler)
    {
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<EmployeeCDListResponse> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-cd?page={page}&pageSize={pageSize}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCDListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCDResponse> GetByEidAsync(int eid)
    {
        var response = await _client.GetAsync($"api/employee-cd/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCDResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCDResponse> CreateAsync(EmployeeCDDto employeeCD)
    {
        var json = JsonSerializer.Serialize(employeeCD);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-cd", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCDResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCDResponse> UpdateAsync(int eid, EmployeeCDDto employeeCD)
    {
        var json = JsonSerializer.Serialize(employeeCD);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-cd/{eid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCDResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int eid)
    {
        var response = await _client.DeleteAsync($"api/employee-cd/{eid}");
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

public class EmployeeCDResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeCDDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeCDListResponse
{
    public bool Success { get; set; }
    public List<EmployeeCDDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EmployeeCDDto
{
    public int Eid { get; set; }
    public string IdNumber { get; set; }
    public DateTime IssueDay { get; set; }
    public string IssuePlace { get; set; }
    public string Country { get; set; }
    public EmployeeDto Employee { get; set; }
}