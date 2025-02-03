namespace Credito.ScoreEngine.API.Exceptions;

public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message, Exception inner = null) 
        : base(message, inner)
    {
    }
}