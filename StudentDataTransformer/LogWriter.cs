namespace StudentDataTransformer;

public static class LogWriter
{
    private const string LogFile = "log.txt";

    public static void Log(Exception exception, string message)
    {
        var logMessage = LogMessage(exception);
        Log($"{logMessage}, {message}");
    }
    public static void Log(Exception exception)
    {
        var logMessage = LogMessage(exception);
        Log(logMessage);
    }
    
    public static void Log(string log)
    {
        File.AppendAllText(LogFile, $"{log}\n");
    }

    public static void Cleanup()
    {
        File.Delete(LogFile);
    }

    private static string LogMessage(Exception exception)
    {
        var message = exception.Message;
        var type = exception.GetType();
        return $"Wystapil wyjatek {type}: {message}";
    }
}