namespace Dynsec;

public class DynsecTimeoutException : OperationCanceledException
{
    public DynsecTimeoutException(TimeSpan timeout, Exception innerException):base($"Operation timed out after {timeout.TotalMilliseconds}ms", innerException)
    {
    }
}