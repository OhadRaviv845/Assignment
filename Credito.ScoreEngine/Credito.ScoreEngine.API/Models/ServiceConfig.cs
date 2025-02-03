namespace Credito.ScoreEngine.API.Models;

public class ServiceConfig
{
    public string Name { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}