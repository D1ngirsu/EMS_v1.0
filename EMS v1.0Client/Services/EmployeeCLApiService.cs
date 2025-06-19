using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeCLService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeCLService(string baseUrl)
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

    public async Task<EmployeeCLListResponse> GetAllAsync()
    {
        var response = await _client.GetAsync("api/employee-cl");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCLListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCLResponse> GetByCidAsync(int cid)
    {
        var response = await _client.GetAsync($"api/employee-cl/{cid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCLResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCLListResponse> GetByEidAsync(int eid)
    {
        var response = await _client.GetAsync($"api/employee-cl/eid/{eid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCLListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCLResponse> CreateAsync(EmployeeCLDto employeeCL)
    {
        var json = JsonSerializer.Serialize(employeeCL);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-cl", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCLResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCLResponse> UpdateAsync(int cid, EmployeeCLDto employeeCL)
    {
        var json = JsonSerializer.Serialize(employeeCL);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-cl/{cid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCLResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int cid)
    {
        var response = await _client.DeleteAsync($"api/employee-cl/{cid}");
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

public class EmployeeCLResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeCLDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeCLListResponse
{
    public bool Success { get; set; }
    public List<EmployeeCLDto> Data { get; set; }
}
public class EmployeeCLDto
{
    public int Cid { get; set; }
    public int Eid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public string Status { get; set; }
    public string EmployeeUser { get; set; }
    public DateTime SignDate { get; set; }
    public string Img { get; set; }
    public EmployeeDto Employee { get; set; }
}