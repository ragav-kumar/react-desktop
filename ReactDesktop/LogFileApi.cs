using System.IO;

namespace ReactDesktop;

public static class LogFileApi
{
    private static readonly Lock logLock = new();
    
    public static void WriteLine(string? message)
    {
        // Write message to log file
        string? exePath = Environment.ProcessPath;
        string baseDir = exePath is not null
            ? Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory
            : AppContext.BaseDirectory;

        string exeFileName = exePath is not null
            ? Path.GetFileName(exePath) // e.g. "MyApp.exe"
            : "Application.exe";

        string logPath = Path.Combine(baseDir, exeFileName + ".log"); // "MyApp.exe.log"

        lock (logLock)
        {
            Directory.CreateDirectory(baseDir); // no-op if it already exists

            using FileStream stream = new(
                logPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite
            );

            using StreamWriter writer = new(stream);
            writer.WriteLine(message ?? string.Empty);
        }
    }

    public static string[] ReadAllLogLines() => ReadLogLines(0, int.MaxValue);
    
    public static string[] ReadLogLines(int skip, int take)
    {
        if (skip < 0)
            skip = 0;
        if (take <= 0)
            return [];

        string? exePath = Environment.ProcessPath;
        string baseDir = exePath is not null
            ? Path.GetDirectoryName(exePath) ?? AppContext.BaseDirectory
            : AppContext.BaseDirectory;

        string exeFileName = exePath is not null
            ? Path.GetFileName(exePath)
            : "Application.exe";

        string logPath = Path.Combine(baseDir, exeFileName + ".log");
        
        lock (logLock)
        {
            // The file might be created/deleted between the Exists check and the lock acquisition.
            if (!File.Exists(logPath))
                return [];

            using FileStream stream = new(
                logPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );

            using StreamReader reader = new(stream);

            var result = new List<string>(capacity: Math.Min(take, 256));

            for (int i = 0; i < skip; i++)
            {
                if (reader.ReadLine() is null)
                    return [];
            }

            for (int i = 0; i < take; i++)
            {
                string? line = reader.ReadLine();
                if (line is null)
                    break;
                result.Add(line);
            }

            return result.ToArray();
        }
    }
}
