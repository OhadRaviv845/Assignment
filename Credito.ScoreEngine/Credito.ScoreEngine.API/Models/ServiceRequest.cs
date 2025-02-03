namespace Credito.ScoreEngine.API.Models;

public class ServiceRequest
{
    public string ServiceName { get; set; } = "";
    public Dictionary<string, object> Payload { get; set; } = new();
}