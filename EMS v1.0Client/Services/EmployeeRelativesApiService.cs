using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeRelativesService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeRelativesService(string baseUrl)
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

    public async Task<EmployeeRelativesListResponse> GetAllAsync(int? eid = null)
    {
        var queryString = eid.HasValue ? $"?eid={eid.Value}" : "";
        var response = await _client.GetAsync($"api/employee-relatives{queryString}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeRelativesListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeRelativesResponse> GetByIdAsync(int relId)
    {
        var response = await _client.GetAsync($"api/employee-relatives/{relId}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeRelativesResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeRelativesListResponse> GetByEmployeeIdAsync(int eid)
    {
        var response = await _client.GetAsync($"api/employee-relatives/by-employee/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeRelativesListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeRelativesResponse> CreateAsync(EmployeeRelativesDto relative)
    {
        var json = JsonSerializer.Serialize(relative);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-relatives", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeRelativesResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeRelativesResponse> UpdateAsync(int relId, EmployeeRelativesDto relative)
    {
        var json = JsonSerializer.Serialize(relative);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-relatives/{relId}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeRelativesResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int relId)
    {
        var response = await _client.DeleteAsync($"api/employee-relatives/{relId}");
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

public class EmployeeRelativesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeRelativesDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeRelativesListResponse
{
    public bool Success { get; set; }
    public List<EmployeeRelativesDto> Data { get; set; }
}

public class EmployeeRelativesDto
{
    public int RelId { get; set; }
    public int Eid { get; set; }
    public string RName { get; set; }
    public string RRelativity { get; set; }
    public string RContact { get; set; }
    public byte Type { get; set; }
    public EmployeeDto Employee { get; set; }
}