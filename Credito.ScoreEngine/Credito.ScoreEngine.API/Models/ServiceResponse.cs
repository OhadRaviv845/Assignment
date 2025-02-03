namespace Credito.ScoreEngine.API.Models;

public class ServiceResponse
{
    public string ExecutionId { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, object> Result { get; set; } = new();
    public string Status { get; set; } = "Success";
    public TimeSpan ExecutionTime { get; set; }
}