﻿using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class EmployeeInsuranceService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public EmployeeInsuranceService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
    }

    public async Task<EmployeeInsuranceListResponse> GetInsurancesAsync(
        string? name = null,
        string? unitName = null,
        string? sortOrder = null, // "asc", "desc", "no-insurance"
        int page = 1,
        int pageSize = 10)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(name)) queryParams.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(unitName)) queryParams.Add($"unitName={Uri.EscapeDataString(unitName)}");
        if (!string.IsNullOrEmpty(sortOrder)) queryParams.Add($"sortOrder={Uri.EscapeDataString(sortOrder)}");
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
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
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
    public List<EmployeeInsuranceListDto> Data { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class EmployeeInsuranceListDto
{
    public int? Iid { get; set; }
    public int Eid { get; set; }
    public string EmployeeName { get; set; }
    public string PositionName { get; set; }
    public string UnitName { get; set; }
    public string? InsuranceContent { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? ContributePercent { get; set; }
}