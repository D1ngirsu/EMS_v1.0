using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeTodolistService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeTodolistService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
    }

    public async Task<EmployeeTodolistListResponse> GetAllAsync(int? eid = null, byte? status = null, int page = 1, int pageSize = 10)
    {
        var queryParams = new List<string>();
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");
        if (eid.HasValue) queryParams.Add($"eid={eid.Value}");
        if (status.HasValue) queryParams.Add($"status={status.Value}");

        var queryString = string.Join("&", queryParams);
        var response = await _client.GetAsync($"api/employee-todolist?{queryString}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeTodolistListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeTodolistResponse> GetByIdAsync(int tid)
    {
        var response = await _client.GetAsync($"api/employee-todolist/{tid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeTodolistResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeTodolistResponse> CreateAsync(EmployeeTodolistDto todo)
    {
        var json = JsonSerializer.Serialize(todo);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-todolist", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeTodolistResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeTodolistResponse> UpdateAsync(int tid, EmployeeTodolistDto todo)
    {
        var json = JsonSerializer.Serialize(todo);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-todolist/{tid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeTodolistResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int tid)
    {
        var response = await _client.DeleteAsync($"api/employee-todolist/{tid}");
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

public class EmployeeTodolistResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeTodolistDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeTodolistListResponse
{
    public bool Success { get; set; }
    public List<EmployeeTodolistDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string Message { get; internal set; }
}
