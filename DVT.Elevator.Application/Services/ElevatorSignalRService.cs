using DVT.Elevator.Application.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DVT.Elevator.Application.Services;

/// <summary>
/// Manages the SignalR connection to the API hub and raises events
/// when the server pushes real-time elevator updates.
/// </summary>
public class ElevatorSignalRService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly ILogger<ElevatorSignalRService> _logger;

    // ─── Events raised when the server pushes updates ─────────────────────────

    public event Action<ElevatorStatusModel>? OnElevatorStatusChanged;
    public event Action<ElevatorMovedModel>? OnElevatorMoved;
    public event Action<string>? OnNewRequest;
    public event Action<int>? OnRequestCompleted;
    public event Action<string>? OnCapacityWarning;
    public event Action<string>? OnConnectionStateChanged;

    public bool IsConnected => _connection.State == HubConnectionState.Connected;

    public ElevatorSignalRService(IConfiguration configuration, ILogger<ElevatorSignalRService> logger)
    {
        _logger = logger;

        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7083";
        var hubUrl = $"{baseUrl}/hubs/elevator";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Accept self-signed dev certificates
                options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
            .Build();

        RegisterHandlers();
        RegisterConnectionEvents();
    }

    // ─── Connection Management ────────────────────────────────────────────────

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.StartAsync(cancellationToken);
            _logger.LogInformation("SignalR connected to hub");
            OnConnectionStateChanged?.Invoke("Connected");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SignalR connection failed — will retry automatically");
            OnConnectionStateChanged?.Invoke("Disconnected");
        }
    }

    public async Task SubscribeToBuildingAsync(int buildingId)
    {
        if (!IsConnected) return;

        try
        {
            await _connection.InvokeAsync("SubscribeToBuilding", buildingId);
            _logger.LogInformation("Subscribed to building {BuildingId}", buildingId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to subscribe to building {BuildingId}", buildingId);
        }
    }

    public async Task UnsubscribeFromBuildingAsync(int buildingId)
    {
        if (!IsConnected) return;

        try
        {
            await _connection.InvokeAsync("UnsubscribeFromBuilding", buildingId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to unsubscribe from building {BuildingId}", buildingId);
        }
    }

    // ─── Server → Client Handlers ─────────────────────────────────────────────

    private void RegisterHandlers()
    {
        // Elevator status changed (full status object)
        _connection.On<JsonElement>("ElevatorStatusChanged", data =>
        {
            try
            {
                var status = JsonSerializer.Deserialize<ElevatorStatusModel>(data.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (status != null)
                    OnElevatorStatusChanged?.Invoke(status);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize ElevatorStatusChanged");
            }
        });

        // Elevator moved to a new floor
        _connection.On<JsonElement>("ElevatorMoved", data =>
        {
            try
            {
                var moved = JsonSerializer.Deserialize<ElevatorMovedModel>(data.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (moved != null)
                    OnElevatorMoved?.Invoke(moved);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize ElevatorMoved");
            }
        });

        // New passenger request created
        _connection.On<JsonElement>("NewRequest", data =>
        {
            try
            {
                OnNewRequest?.Invoke(data.GetRawText());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to handle NewRequest");
            }
        });

        // Request completed
        _connection.On<JsonElement>("RequestCompleted", data =>
        {
            try
            {
                var requestId = data.GetProperty("requestId").GetInt32();
                OnRequestCompleted?.Invoke(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to handle RequestCompleted");
            }
        });

        // Capacity warning
        _connection.On<JsonElement>("CapacityWarning", data =>
        {
            try
            {
                var message = data.GetProperty("message").GetString() ?? "Capacity warning";
                OnCapacityWarning?.Invoke(message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to handle CapacityWarning");
            }
        });
    }

    private void RegisterConnectionEvents()
    {
        _connection.Reconnecting += error =>
        {
            _logger.LogWarning("SignalR reconnecting: {Error}", error?.Message);
            OnConnectionStateChanged?.Invoke("Reconnecting...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR reconnected: {ConnectionId}", connectionId);
            OnConnectionStateChanged?.Invoke("Connected");
            return Task.CompletedTask;
        };

        _connection.Closed += error =>
        {
            _logger.LogWarning("SignalR connection closed: {Error}", error?.Message);
            OnConnectionStateChanged?.Invoke("Disconnected");
            return Task.CompletedTask;
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
