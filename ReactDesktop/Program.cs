using System.CommandLine;
using ReactDesktop;

// ReSharper disable once CheckNamespace
internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        RootCommand root = new("Demonstration of React UI built within a WPF app.");

        Option<bool?> modeOption = new("--silent");
        Option<bool?> readOption = new("--read");
        Option<string> connectionStringOption = new("--connectionString");
        root.Add(modeOption);
        root.Add(connectionStringOption);
        
        root.SetAction(result =>
        {
            bool isSilent = result.GetValue(modeOption) ?? false;
            CommandParams commandParams = new(
                result.GetValue(connectionStringOption),
                result.GetValue(readOption) ?? false
            );
    
            return isSilent
                ? RunCli(commandParams)
                : RunUi(commandParams);
        });

        ParseResult parseResult = root.Parse(args);
        return parseResult.Invoke();
    }

    private static int RunCli(CommandParams commandParams)
    {
        if (commandParams.IsReadMode)
        {
            foreach (string line in BusinessLogic.ReadAllLogLines())
            {
                Console.WriteLine(line);
            }
        }
        else
        {
            BusinessLogic.WriteLine($"Ran in CLI mode with connection string: {commandParams.ConnectionString}");
        }
        return 0;
    }

    private static int RunUi(CommandParams commandParams)
    {
        try
        {
            App app = new();
            app.Run(new MainWindow(commandParams));

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }
    }
}
