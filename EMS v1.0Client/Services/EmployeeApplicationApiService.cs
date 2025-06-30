using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


public class EmployeeApplicationService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeApplicationService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
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
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
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
    public string Message { get; internal set; }
}
