using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;

public class EmployeeASService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeASService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
    }

    public async Task<EmployeeASListResponse> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-as?page={page}&pageSize={pageSize}");
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

    public async Task<EmployeeASResponse> CreateAsync(EmployeeASDto employeeAS, byte[] imageData1 = null, byte[] imageData2 = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(employeeAS), Encoding.UTF8, "application/json"), "employeeAS");
        if (imageData1 != null)
        {
            content.Add(new ByteArrayContent(imageData1), "image1", $"image_{Guid.NewGuid()}.jpg");
        }
        if (imageData2 != null)
        {
            content.Add(new ByteArrayContent(imageData2), "image2", $"image_{Guid.NewGuid()}.jpg");
        }

        var response = await _client.PostAsync("api/employee-as", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeASResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeASResponse> UpdateAsync(int eid, EmployeeASDto employeeAS, byte[] imageData1 = null, byte[] imageData2 = null)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(employeeAS), Encoding.UTF8, "application/json"), "employeeAS");
        if (imageData1 != null)
        {
            content.Add(new ByteArrayContent(imageData1), "image1", $"image_{Guid.NewGuid()}.jpg");
        }
        if (imageData2 != null)
        {
            content.Add(new ByteArrayContent(imageData2), "image2", $"image_{Guid.NewGuid()}.jpg");
        }

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
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
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
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
