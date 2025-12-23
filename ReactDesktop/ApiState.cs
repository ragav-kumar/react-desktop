namespace ReactDesktop;

/// <summary>
/// The C# side state of our application.
/// This would likely exist as a go-between for the React app and any file or system behaviours.
/// </summary>
public class ApiState
{
    public string? ConnectionString
    {
        get
        {
            BusinessLogic.WriteLine($"Connection string read. Current value: \"{field}\".");
            return field;
        }
        set
        {
            field = value;
            BusinessLogic.WriteLine($"Connection string modified to: \"{value}\".");
        }
    }

    public ApiState(string? connectionString)
    {
        ConnectionString = connectionString;
    }
    
    
}
