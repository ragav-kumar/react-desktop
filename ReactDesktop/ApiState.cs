namespace ReactDesktop;

/// <summary>
/// The C# side state of our application.
/// This would likely exist as a go-between for the React app and any file or system behaviours.
/// </summary>
public class ApiState
{
    private string? _connectionString;

    public ApiState(string? connectionString)
    {
        _connectionString = connectionString;
    }
    
    
}
