namespace StudentDataTransformer;

public class LogWriter
{
    private static readonly string Directory = System.IO.Directory.GetCurrentDirectory();
    private static readonly string LogFile = $"{Directory}log.txt";
    
    public static void Log(Exception exception)
    {
        var logMessage = LogMessage(exception);
        Log(logMessage);
    }
    
    public static void Log(string log)
    {
        File.WriteAllText(LogFile, log);
    }

    private static string LogMessage(Exception exception)
    {
        var message = exception.Message;
        var type = exception.GetType();
        return $"Wystapil wyjatek {type}: {message}";
    }
}