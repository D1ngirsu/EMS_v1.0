using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;

public class EmployeeApplicationService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeApplicationService(string baseUrl, HttpClientHandler handler)
    {
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<EmployeeApplicationListResponse> GetAllAsync(int? eid = null, int page = 1, int pageSize = 10)
    {
        var queryParams = new List<string>();
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");
        if (eid.HasValue) queryParams.Add($"eid={eid.Value}");

        var queryString = string.Join("&", queryParams);
        var response = await _client.GetAsync($"api/employee-application?{queryString}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeApplicationListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeApplicationResponse> GetByIdAsync(int appId)
    {
        var response = await _client.GetAsync($"api/employee-application/{appId}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeApplicationResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeApplicationListResponse> GetByEmployeeIdAsync(int eid, int page = 1, int pageSize = 10)
    {
        var queryParams = new List<string>();
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var queryString = string.Join("&", queryParams);
        var response = await _client.GetAsync($"api/employee-application/employee/{eid}?{queryString}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeApplicationListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeApplicationResponse> CreateAsync(EmployeeApplicationDto application, byte[] imageData = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(application), Encoding.UTF8, "application/json"), "application");
        if (imageData != null)
        {
            content.Add(new ByteArrayContent(imageData), "image", $"image_{Guid.NewGuid()}.jpg");
        }

        var response = await _client.PostAsync("api/employee-application", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeApplicationResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeApplicationResponse> UpdateAsync(int appId, EmployeeApplicationDto application, byte[] imageData = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(application), Encoding.UTF8, "application/json"), "application");
        if (imageData != null)
        {
            content.Add(new ByteArrayContent(imageData), "image", $"image_{Guid.NewGuid()}.jpg");
        }

        var response = await _client.PutAsync($"api/employee-application/{appId}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeApplicationResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int appId)
    {
        var response = await _client.DeleteAsync($"api/employee-application/{appId}");
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

public class EmployeeApplicationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeApplicationDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeApplicationListResponse
{
    public bool Success { get; set; }
    public List<EmployeeApplicationDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EmployeeApplicationDto
{
    public int AppId { get; set; }
    public int Eid { get; set; }
    public string? ApplicationName { get; set; }
    public DateTime Date { get; set; }
    public string? Img { get; set; }
    public string? ApplicationType { get; set; }
    public EmployeeDto? Employee { get; set; }
}