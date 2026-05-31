using DVT.Elevator.Application.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace DVT.Elevator.Application.Services;

public class ElevatorApiClient : IElevatorApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ElevatorApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ─── Buildings ────────────────────────────────────────────────────────────

    public async Task<List<BuildingModel>> GetBuildingsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<BuildingModel>>(
                "api/buildings", _jsonOptions);
            return result ?? new List<BuildingModel>();
        }
        catch { return new List<BuildingModel>(); }
    }

    public async Task<BuildingModel?> GetBuildingByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BuildingModel>(
                $"api/buildings/{id}", _jsonOptions);
        }
        catch { return null; }
    }

    public async Task<BuildingModel?> CreateBuildingAsync(CreateBuildingModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/buildings", model);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<BuildingModel>(_jsonOptions);

            var error = await response.Content.ReadFromJsonAsync<ApiErrorModel>(_jsonOptions);
            throw new Exception(error?.Message ?? $"API error: {response.StatusCode}");
        }
        catch (Exception ex) when (ex.Message.Contains("API error") || ex.Message.Contains("name") || ex.Message.Contains("floor"))
        {
            throw;
        }
        catch { return null; }
    }

    // ─── Elevators ────────────────────────────────────────────────────────────

    public async Task<List<ElevatorModel>> GetElevatorsByBuildingAsync(int buildingId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<ElevatorModel>>(
                $"api/elevators/building/{buildingId}", _jsonOptions);
            return result ?? new List<ElevatorModel>();
        }
        catch { return new List<ElevatorModel>(); }
    }

    public async Task<List<ElevatorStatusModel>> GetElevatorStatusesAsync(int buildingId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<ElevatorStatusModel>>(
                $"api/elevators/building/{buildingId}/statuses", _jsonOptions);
            return result ?? new List<ElevatorStatusModel>();
        }
        catch { return new List<ElevatorStatusModel>(); }
    }

    public async Task<ElevatorStatusModel?> GetElevatorStatusAsync(int elevatorId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ElevatorStatusModel>(
                $"api/elevators/{elevatorId}/status", _jsonOptions);
        }
        catch { return null; }
    }

    public async Task<bool> SetMaintenanceModeAsync(int elevatorId, bool inMaintenance)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/elevators/{elevatorId}/maintenance", inMaintenance);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<ElevatorModel?> CreateElevatorAsync(CreateElevatorModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/elevators", model);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ElevatorModel>(_jsonOptions);

            var error = await response.Content.ReadFromJsonAsync<ApiErrorModel>(_jsonOptions);
            throw new Exception(error?.Message ?? $"API error: {response.StatusCode}");
        }
        catch (Exception ex) when (ex.Message.Contains("API error") || ex.Message.Contains("name") || ex.Message.Contains("capacity"))
        {
            throw;
        }
        catch { return null; }
    }

    public async Task<List<ElevatorTypeModel>> GetElevatorTypesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<ElevatorTypeModel>>(
                "api/elevatortypes", _jsonOptions);
            return result ?? new List<ElevatorTypeModel>();
        }
        catch { return new List<ElevatorTypeModel>(); }
    }

    // ─── Passenger Requests ───────────────────────────────────────────────────

    public async Task<PassengerRequestResponseModel?> RequestElevatorAsync(CreatePassengerRequestModel request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/passengerrequests", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<PassengerRequestResponseModel>(_jsonOptions);

            var error = await response.Content.ReadFromJsonAsync<ApiErrorModel>(_jsonOptions);
            throw new Exception(error?.Message ?? $"API error: {response.StatusCode}");
        }
        catch (Exception ex) when (ex.Message.StartsWith("API error") || ex.Message.Contains("floor") || ex.Message.Contains("passenger"))
        {
            throw;
        }
        catch { return null; }
    }

    public async Task<List<PassengerRequestModel>> GetActiveRequestsAsync(int buildingId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<PassengerRequestModel>>(
                $"api/passengerrequests/building/{buildingId}/active", _jsonOptions);
            return result ?? new List<PassengerRequestModel>();
        }
        catch { return new List<PassengerRequestModel>(); }
    }

    public async Task<List<PassengerRequestModel>> GetRequestHistoryAsync(int buildingId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<PassengerRequestModel>>(
                $"api/passengerrequests/building/{buildingId}/history", _jsonOptions);
            return result ?? new List<PassengerRequestModel>();
        }
        catch { return new List<PassengerRequestModel>(); }
    }

    // ─── Health ───────────────────────────────────────────────────────────────

    public async Task<bool> IsApiHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("health");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
