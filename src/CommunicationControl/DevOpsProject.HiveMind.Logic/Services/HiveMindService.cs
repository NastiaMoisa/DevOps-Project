using DevOpsProject.HiveMind.Logic.Services.Interfaces;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Models;
using DevOpsProject.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using DevOpsProject.HiveMind.Logic.State;
using System.Text;
using Polly;
using System.Net;

namespace DevOpsProject.HiveMind.Logic.Services
{
    public class HiveMindService : IHiveMindService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HiveMindHttpClient _httpClient;
        private readonly ILogger<HiveMindService> _logger;
        private readonly HiveCommunicationConfig _communicationConfigurationOptions;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private Timer? _telemetryTimer;
        private volatile bool _isConnected;

        public HiveMindService(IHttpClientFactory httpClientFactory, HiveMindHttpClient httpClient, ILogger<HiveMindService> logger, IOptionsSnapshot<HiveCommunicationConfig> communicationConfigurationOptions)
        {
            _httpClient = httpClient;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _communicationConfigurationOptions = communicationConfigurationOptions.Value;
        }

        public async Task ConnectHive()
        {
            await EnsureConnectedAsync();
        }

        public bool AddInterference(InterferenceModel interferenceModel)
        {
            var isAdded = HiveInMemoryState.AddInterference(interferenceModel);
            return isAdded;
        }

        public void RemoveInterference(Guid interferenceId)
        {
            HiveInMemoryState.RemoveInterference(interferenceId);
        }

        public void StopAllTelemetry()
        {
            StopTelemetry();
        }

        #region private methods
        private async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
        {
            if (_isConnected)
            {
                return;
            }

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_isConnected)
                {
                    return;
                }

                var request = new HiveConnectRequest
                {
                    HiveSchema = _communicationConfigurationOptions.RequestSchema,
                    HiveIP = _communicationConfigurationOptions.HiveIP,
                    HivePort = _communicationConfigurationOptions.HivePort,
                    HiveID = _communicationConfigurationOptions.HiveID
                };

                var uriBuilder = new UriBuilder
                {
                    Scheme = _communicationConfigurationOptions.RequestSchema,
                    Host = _communicationConfigurationOptions.CommunicationControlIP,
                    Port = _communicationConfigurationOptions.CommunicationControlPort,
                    Path = $"{_communicationConfigurationOptions.CommunicationControlPath}/connect"
                };

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Attempting to connect Hive. HiveID: {hiveId}, URI: {uri}", request.HiveID, uriBuilder.Uri);

                        var response = await _httpClient.SendCommunicationControlConnectAsync(
                            _communicationConfigurationOptions.RequestSchema,
                            _communicationConfigurationOptions.CommunicationControlIP,
                            _communicationConfigurationOptions.CommunicationControlPort,
                            _communicationConfigurationOptions.CommunicationControlPath,
                            request);

                        if (!response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                            _logger.LogWarning(
                                "Hive connect failed with status {statusCode}. Response: {responseBody}. Retrying in 5 seconds.",
                                (int)response.StatusCode,
                                responseBody);
                            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                            continue;
                        }

                        var connectResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                        var hiveConnectResponse = JsonSerializer.Deserialize<HiveConnectResponse>(connectResponse);

                        if (hiveConnectResponse?.ConnectResult != true)
                        {
                            _logger.LogWarning("Hive connect returned an unsuccessful payload. Retrying in 5 seconds.");
                            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                            continue;
                        }

                        HiveInMemoryState.OperationalArea = hiveConnectResponse.OperationalArea;
                        HiveInMemoryState.CurrentLocation = _communicationConfigurationOptions.InitialLocation;
                        HiveInMemoryState.Interferences = hiveConnectResponse.Interferences ?? [];
                        _isConnected = true;
                        StartTelemetry();

                        _logger.LogInformation("Hive connected successfully. HiveID: {hiveId}", request.HiveID);
                        return;
                    }
                    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                    {
                        _logger.LogWarning(ex, "Hive connect failed. Retrying in 5 seconds.");
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    }
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private void StartTelemetry()
        {
            if (HiveInMemoryState.IsTelemetryRunning) return;

            HiveInMemoryState.IsTelemetryRunning = true;
            _telemetryTimer = new Timer(SendTelemetry, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

            _logger.LogInformation("Telemetry timer started.");
        }

        private void StopTelemetry()
        {
            _telemetryTimer?.Dispose();
            _telemetryTimer = null;
            HiveInMemoryState.IsTelemetryRunning = false;
            _isConnected = false;

            _logger.LogInformation("Telemetry timer stopped.");
        }

        private async void SendTelemetry(object state)
        {
            try
            {
                if (!_isConnected)
                {
                    await EnsureConnectedAsync();
                }

                var request = new HiveTelemetryRequest
                {
                    HiveID = _communicationConfigurationOptions.HiveID,
                    Location = HiveInMemoryState.CurrentLocation ?? default,
                    // TODO: MOCKED FOR NOW
                    Height = 5,
                    Speed = 15,
                    State = Shared.Enums.HiveMindState.Move
                };

                var response = await _httpClient.SendCommunicationControlTelemetryAsync(_communicationConfigurationOptions.RequestSchema,
                    _communicationConfigurationOptions.CommunicationControlIP, _communicationConfigurationOptions.CommunicationControlPort,
                    _communicationConfigurationOptions.CommunicationControlPath, request);

                if (response.IsSuccessStatusCode)
                {
                    var connectResult = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Telemetry sent for HiveID: {hiveId}: {response}", request.HiveID, connectResult);
                    return;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(
                        "Telemetry rejected because hive is not connected yet. HiveID: {hiveId}. Response: {responseBody}. Reconnecting.",
                        request.HiveID,
                        responseBody);
                    _isConnected = false;
                    await EnsureConnectedAsync();
                    return;
                }

                _logger.LogError(
                    "Failed to send telemetry for HiveID: {hiveId}. Status: {statusCode}. Response: {responseBody}",
                    request.HiveID,
                    (int)response.StatusCode,
                    responseBody);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Error sending telemetry. Reconnecting Hive.");
                _isConnected = false;
                await EnsureConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending telemetry.");
            }
        }
        #endregion
    }
}
