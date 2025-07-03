using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OrganizationApiService : IDisposable
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _disposed;

    public OrganizationApiService(string baseUrl, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _client = httpClientFactory.CreateClient(baseUrl);
    }

    // Position Methods

    public async Task<ApiResponse<List<PositionDto>>> GetPositionsAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/organization/positions");
            response.EnsureSuccessStatusCode(); // Throw if response is not successful
            var responseJson = await response.Content.ReadAsStringAsync();
            var positions = JsonSerializer.Deserialize<List<PositionDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return new ApiResponse<List<PositionDto>>
            {
                Success = true,
                Data = positions,
                Message = "Positions retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PositionDto>>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<PositionDto>> GetPositionAsync(int id)
    {
        try
        {
            var response = await _client.GetAsync($"api/organization/positions/{id}");
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<PositionDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<PositionDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<PositionDto>> CreatePositionAsync(PositionDto positionDto)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(positionDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/organization/positions", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<PositionDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<PositionDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<PositionDto>> UpdatePositionAsync(int id, PositionDto positionDto)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(positionDto), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"api/organization/positions/{id}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<PositionDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<PositionDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<GenericResponse> DeletePositionAsync(int id)
    {
        try
        {
            var response = await _client.DeleteAsync($"api/organization/positions/{id}");
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    // OrganizationUnit Methods

    public async Task<ApiResponse<List<OrganizationUnitDto>>> GetDepartmentsAsync()
    {
        try
        {
            var response = await _client.GetAsync("api/organization/departments");
            response.EnsureSuccessStatusCode(); // Throw if response is not successful
            var responseJson = await response.Content.ReadAsStringAsync();
            var departments = JsonSerializer.Deserialize<List<OrganizationUnitDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return new ApiResponse<List<OrganizationUnitDto>>
            {
                Success = true,
                Data = departments,
                Message = "Departments retrieved successfully"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<OrganizationUnitDto>>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<List<OrganizationUnitDto>>> GetGroupsByDepartmentAsync(int departmentId)
    {
        try
        {
            var response = await _client.GetAsync($"api/organization/departments/{departmentId}/groups");
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<List<OrganizationUnitDto>>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<OrganizationUnitDto>>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<OrganizationUnitDto>> GetOrganizationUnitAsync(int id)
    {
        try
        {
            var response = await _client.GetAsync($"api/organization/organization-units/{id}");
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<OrganizationUnitDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<OrganizationUnitDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<OrganizationUnitDto>> CreateOrganizationUnitAsync(OrganizationUnitDto unitDto)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(unitDto), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/organization/organization-units", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<OrganizationUnitDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<OrganizationUnitDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<OrganizationUnitDto>> UpdateOrganizationUnitAsync(int id, OrganizationUnitDto unitDto)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(unitDto), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"api/organization/organization-units/{id}", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ApiResponse<OrganizationUnitDto>>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new ApiResponse<OrganizationUnitDto>
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<GenericResponse> DeleteOrganizationUnitAsync(int id)
    {
        try
        {
            var response = await _client.DeleteAsync($"api/organization/organization-units/{id}");
            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<GenericResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _client?.Dispose();
        _disposed = true;
    }
}