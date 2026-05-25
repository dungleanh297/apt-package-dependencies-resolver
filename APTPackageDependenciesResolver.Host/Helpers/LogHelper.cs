public static class LogHelper
{
    public static void LogError(string message)
    {
        Console.Error.WriteLine("ERROR: {0}", message);
    }

    public static void LogWarning(string message)
    {
        Console.Error.WriteLine("WARNING: {0}", message);
    }
}