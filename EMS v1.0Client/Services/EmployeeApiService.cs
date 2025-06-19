using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeApiService(string baseUrl)
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

    public async Task<EmployeeResponse> CreateEmployeeAsync(Employee employee)
    {
        var json = JsonSerializer.Serialize(employee);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

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
        var response = await _client.GetAsync($"api/employee/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int eid, Employee employee)
    {
        var json = JsonSerializer.Serialize(employee);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

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