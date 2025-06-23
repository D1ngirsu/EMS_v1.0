using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeInsuranceService : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public EmployeeInsuranceService(string baseUrl, HttpClientHandler handler)
    {
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<EmployeeInsuranceListResponse> GetInsurancesAsync(
        string? name = null, int? eid = null, int? iid = null, int page = 1, int pageSize = 10)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
        if (eid.HasValue) queryParams.Add($"eid={eid.Value}");
        if (iid.HasValue) queryParams.Add($"iid={iid.Value}");
        queryParams.Add($"page={page}");
        queryParams.Add($"pageSize={pageSize}");

        var queryString = string.Join("&", queryParams);
        var response = await _client.GetAsync($"api/employee-insurance{(queryString.Length > 0 ? $"?{queryString}" : "")}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeInsuranceListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeInsuranceResponse> GetByIidAsync(int iid)
    {
        var response = await _client.GetAsync($"api/employee-insurance/{iid}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeInsuranceResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeInsuranceListResponse> GetByEidAsync(int eid, int page = 1, int pageSize = 10)
    {
        var response = await _client.GetAsync($"api/employee-insurance/eid/{eid}?page={page}&pageSize={pageSize}");
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeInsuranceListResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeInsuranceResponse> CreateAsync(EmployeeInsuranceDto employeeInsurance)
    {
        var json = JsonSerializer.Serialize(employeeInsurance);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("api/employee-insurance", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeInsuranceResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<EmployeeInsuranceResponse> UpdateAsync(int iid, EmployeeInsuranceDto employeeInsurance)
    {
        var json = JsonSerializer.Serialize(employeeInsurance);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"api/employee-insurance/{iid}", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<EmployeeInsuranceResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<GenericResponse> DeleteAsync(int iid)
    {
        var response = await _client.DeleteAsync($"api/employee-insurance/{iid}");
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

public class EmployeeInsuranceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public EmployeeInsuranceDto Data { get; set; }
    public object Errors { get; set; }
}

public class EmployeeInsuranceListResponse
{
    public bool Success { get; set; }
    public List<EmployeeInsuranceDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EmployeeInsuranceDto
{
    public int Iid { get; set; }
    public int Eid { get; set; }
    public string InsuranceContent { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal ContributePercent { get; set; }
    public EmployeeDto Employee { get; set; }
}