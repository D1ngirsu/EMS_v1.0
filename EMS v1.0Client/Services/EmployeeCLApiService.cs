using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.IO;

public class EmployeeCLService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeCLService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
        _client.BaseAddress = new Uri(baseUrl);
    }

    public async Task<EmployeeCLListResponse> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-cl?page={page}&pageSize={pageSize}");
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

    public async Task<EmployeeCLListResponse> GetByEidAsync(int eid, int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-cl/eid/{eid}?page={page}&pageSize={pageSize}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeCLListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeCLResponse> CreateAsync(EmployeeCLDto employeeCL, byte[] imageData = null, string imageFileName = null)
    {
        using var content = new MultipartFormDataContent();

        // Add employeeCL fields
        content.Add(new StringContent(employeeCL.Cid.ToString()), "Cid");
        content.Add(new StringContent(employeeCL.Eid.ToString()), "Eid");
        content.Add(new StringContent(employeeCL.StartDate.ToString("yyyy-MM-dd")), "StartDate");
        content.Add(new StringContent(employeeCL.EndDate.ToString("yyyy-MM-dd")), "EndDate");
        if (employeeCL.ExpectedEndDate.HasValue)
        {
            content.Add(new StringContent(employeeCL.ExpectedEndDate.Value.ToString("yyyy-MM-dd")), "ExpectedEndDate");
        }
        content.Add(new StringContent(employeeCL.Status ?? ""), "Status");
        content.Add(new StringContent(employeeCL.EmployeeUser ?? ""), "EmployeeUser");
        content.Add(new StringContent(employeeCL.SignDate.ToString("yyyy-MM-dd")), "SignDate");

        // Handle image
        if (imageData != null && imageData.Length > 0 && !string.IsNullOrEmpty(imageFileName))
        {
            var imageContent = new ByteArrayContent(imageData);
            var extension = Path.GetExtension(imageFileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "image/jpeg"
            };
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "image", imageFileName);
            Console.WriteLine($"Image added to request, length: {imageData.Length} bytes, fileName: {imageFileName}");
        }
        else
        {
            content.Add(new StringContent(""), "Img");
            Console.WriteLine("No image data provided, Img set to empty");
        }

        try
        {
            var response = await _client.PostAsync("api/employee-cl", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<EmployeeCLResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateAsync: {ex.Message}");
            return new EmployeeCLResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<EmployeeCLResponse> UpdateAsync(int cid, EmployeeCLDto employeeCL, byte[] imageData = null, string imageFileName = null)
    {
        using var content = new MultipartFormDataContent();

        // Add employeeCL fields
        content.Add(new StringContent(employeeCL.Cid.ToString()), "Cid");
        content.Add(new StringContent(employeeCL.Eid.ToString()), "Eid");
        content.Add(new StringContent(employeeCL.StartDate.ToString("yyyy-MM-dd")), "StartDate");
        content.Add(new StringContent(employeeCL.EndDate.ToString("yyyy-MM-dd")), "EndDate");
        if (employeeCL.ExpectedEndDate.HasValue)
        {
            content.Add(new StringContent(employeeCL.ExpectedEndDate.Value.ToString("yyyy-MM-dd")), "ExpectedEndDate");
        }
        content.Add(new StringContent(employeeCL.Status ?? ""), "Status");
        content.Add(new StringContent(employeeCL.EmployeeUser ?? ""), "EmployeeUser");
        content.Add(new StringContent(employeeCL.SignDate.ToString("yyyy-MM-dd")), "SignDate");

        // Handle image
        if (imageData != null && imageData.Length > 0 && !string.IsNullOrEmpty(imageFileName))
        {
            var imageContent = new ByteArrayContent(imageData);
            var extension = Path.GetExtension(imageFileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "image/jpeg"
            };
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(imageContent, "image", imageFileName);
            Console.WriteLine($"Image added to request, length: {imageData.Length} bytes, fileName: {imageFileName}");
        }
        else
        {
            content.Add(new StringContent(employeeCL.Img ?? ""), "Img");
            Console.WriteLine($"No new image data provided, Img set to: {employeeCL.Img ?? "null"}");
        }

        try
        {
            var response = await _client.PutAsync($"api/employee-cl/{cid}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<EmployeeCLResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
            return new EmployeeCLResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
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
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
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
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string Message { get; internal set; }
}