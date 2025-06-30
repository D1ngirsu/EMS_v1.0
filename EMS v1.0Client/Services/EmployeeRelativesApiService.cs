using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeRelativesService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeRelativesService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
    }

    public async Task<EmployeeRelativesListResponse> GetAllAsync(int? eid = null, int page = 1, int pageSize = 10)
    {
        var queryParams = new List<string>();
        if (eid.HasValue) queryParams.Add($"eid={eid.Value}");
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var queryString = string.Join("&", queryParams);
        var response = await _client.GetAsync($"api/employee-relatives{(queryString.Length > 0 ? $"?{queryString}" : "")}");
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

    public async Task<EmployeeRelativesListResponse> GetByEmployeeIdAsync(int eid, int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-relatives/by-employee/{eid}?page={page}&pageSize={pageSize}");
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
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
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
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string Message { get; internal set; }
}

