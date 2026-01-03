namespace ReactDesktop.Rpc;

public static class Methods
{
    public const string UiReady = "UiReady";
    public const string GetConnectionString = "GetConnectionString";
    public const string SetConnectionString = "SetConnectionString";
    public const string GetLogLines = "GetLogLines";
    public const string WriteLogLine = "WriteLogLine";
    public const string StartListeningForLogLines = "StartListeningForLogLines";
    public const string StopListeningForLogLines = "StopListeningForLogLines";
    public const string IsListeningForLogLineChanges = "IsListeningForLogLineChanges";
    public const string LogLinesPushNotification = "LogLinesPushNotification";
}
