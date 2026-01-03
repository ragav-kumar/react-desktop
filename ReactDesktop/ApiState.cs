namespace ReactDesktop;

/// <summary>
/// The C# side state of our application.
/// This would likely exist as a go-between for the React app and any file or system behaviours.
/// </summary>
public class ApiState
{
    public bool IsListeningForLogLineChanges { get; set; } = false;
    
    public string? ConnectionString
    {
        get
        {
            LogFileApi.WriteLine($"Connection string read. Current value: \"{field}\".");
            return field;
        }
        set
        {
            field = value;
            LogFileApi.WriteLine($"Connection string modified to: \"{value}\".");
        }
    }

    public ApiState(string? connectionString)
    {
        ConnectionString = connectionString;
    }
}
