using System.IO;
using System.Net.Http;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using ReactDesktop.Rpc;

namespace ReactDesktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private static readonly Uri devUri = new("http://localhost:5173");
    private const string VirtualHost = "app";
    private const string VirtualOrigin = $"https://{VirtualHost}/";
    private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(250);
    
    public MainWindow(CommandParams commandParams)
    {
        InitializeComponent();
        
        _state = new ApiState(commandParams.ConnectionString);
        _methods = new MethodRegistry(this);

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadReactApp();
            await InitializeApi();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task LoadReactApp()
    {
        // In debug mode, we optionally attempt to connect to a dev build of the UI.
#if DEBUG
        if (await IsReachableAsync(devUri))
        {
            Console.WriteLine($"Connected to {devUri}...");
            WebView.Source = devUri;
            return;
        }
#endif
        
        // In prod mode, we instead need to serve the built UI files from the executable folder.
        MapProdUiFolder();
        Console.WriteLine($"Serving UI from {VirtualOrigin}...");
        WebView.Source = new Uri(VirtualOrigin);
    }

    private void MapProdUiFolder()
    {
        // built UI files must be located in the same folder as the EXE.
        // Maybe we could do this by scraping the built UI files when building the project?
        string uiRoot = Path.Combine(AppContext.BaseDirectory, "ui");
        
        WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            VirtualHost,
            uiRoot,
            CoreWebView2HostResourceAccessKind.DenyCors
        );
    }

    private static async Task<bool> IsReachableAsync(Uri uri)
    {
        try
        {
            using CancellationTokenSource cts = new(timeout);
            
            // Avoid DNS delays with localhost; this is usually enough as a “is it up” probe.
            using HttpClient httpClient = new();
            httpClient.Timeout = timeout;

            // Use GET rather than HEAD; some dev servers don't handle HEAD consistently.
            using HttpResponseMessage response = await httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
