using System.Diagnostics;
using System.Net.Http.Json;
using Credito.ScoreEngine.API.Exceptions;
using Credito.ScoreEngine.API.Models;
using Polly.CircuitBreaker;

namespace Credito.ScoreEngine.API.Services;

public class ExternalServiceExecutor
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceExecutor> _logger;
    private readonly Dictionary<string, ServiceConfig> _serviceConfigs;
    private readonly bool _useMockService = true;

    public ExternalServiceExecutor(
        HttpClient httpClient,
        ILogger<ExternalServiceExecutor> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceConfigs = configuration.GetSection("Services")
            .Get<Dictionary<string, ServiceConfig>>() ?? new();
    }

    public async Task<ServiceResponse> ExecuteServiceAsync(ServiceRequest request)
    {
        if (!_serviceConfigs.TryGetValue(request.ServiceName, out var config))
        {
            throw new KeyNotFoundException($"Service {request.ServiceName} not found");
        }

        var stopwatch = Stopwatch.StartNew();
        var executionId = Guid.NewGuid().ToString();
        var retryCount = 0;

        while (retryCount <= config.MaxRetries)
        {
            try
            {
                _logger.LogInformation(
                    "Executing service {ServiceName} with ID {ExecutionId}. Attempt {Attempt}/{MaxRetries}",
                    request.ServiceName, executionId, retryCount + 1, config.MaxRetries);

                Dictionary<string, object> result;
                
                if (_useMockService)
                {
                    result = request.ServiceName switch
                    {
                        "SocioeconomicScoring" => new Dictionary<string, object>
                        {
                            { "score", 750 },
                            { "risk_level", "low" },
                            { "recommendation", "approved" }
                        },
                        "BankStatementAnalyzer" => new Dictionary<string, object>
                        {
                            { "average_balance", 25000 },
                            { "monthly_income", 5000 },
                            { "risk_factor", 0.3 }
                        },
                        _ => throw new NotImplementedException($"Mock for {request.ServiceName} not implemented")
                    };

                    await Task.Delay(500);
                }
                else
                {
                    var response = await _httpClient.PostAsJsonAsync(config.Endpoint, request.Payload);
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>() 
                        ?? new Dictionary<string, object>();
                }

                stopwatch.Stop();

                var serviceResponse = new ServiceResponse
                {
                    ExecutionId = executionId,
                    Result = result,
                    ExecutionTime = stopwatch.Elapsed
                };

                _logger.LogInformation(
                    "Service {ServiceName} executed successfully. ID: {ExecutionId}, Duration: {Duration}ms",
                    request.ServiceName, executionId, stopwatch.ElapsedMilliseconds);

                return serviceResponse;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex, 
                    "Circuit breaker is open for service {ServiceName}. Too many failures detected.", 
                    request.ServiceName);
                throw new ServiceUnavailableException(
                    $"Service {request.ServiceName} is temporarily unavailable due to too many failures", 
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error executing service {ServiceName}. ID: {ExecutionId}, Attempt: {Attempt}/{MaxRetries}",
                    request.ServiceName, executionId, retryCount + 1, config.MaxRetries);

                if (retryCount == config.MaxRetries)
                {
                    throw new Exception($"Service execution failed after {retryCount + 1} attempts", ex);
                }

                await Task.Delay(config.RetryDelayMs);
                retryCount++;
            }
        }

        throw new InvalidOperationException("Unexpected execution flow");
    }
}