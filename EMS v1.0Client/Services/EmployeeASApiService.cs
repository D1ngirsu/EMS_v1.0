using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeASService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeASService(string baseUrl)
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

    public async Task<EmployeeASListResponse> GetAllAsync()
    {
        var response = await _client.GetAsync("api/employee-as");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeASListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeASResponse> GetByEidAsync(int eid)
    {
        var response = await _client.GetAsync($"api/employee-as/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeASResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeASResponse> CreateAsync(EmployeeASDto employeeAS)
    {
        var json = JsonSerializer.Serialize(employeeAS);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-as", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeASResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeASResponse> UpdateAsync(int eid, EmployeeASDto employeeAS)
    {
        var json = JsonSerializer.Serialize(employeeAS);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-as/{eid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeASResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int eid)
    {
        var response = await _client.DeleteAsync($"api/employee-as/{eid}");
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

public class EmployeeASResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeASDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeASListResponse
{
    public bool Success { get; set; }
    public List<EmployeeASDto> Data { get; set; }
}

public class EmployeeASDto
{
    public int Eid { get; set; }
    public string? AcademicRank { get; set; }
    public string? Degree { get; set; }
    public string? PlaceIssue { get; set; }
    public DateTime IssueDay { get; set; }
    public string? DegreeImg1 { get; set; }
    public string? DegreeImg2 { get; set; }
    public EmployeeDto? Employee { get; set; }
}