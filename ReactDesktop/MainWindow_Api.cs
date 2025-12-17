using System.Text.Json;
using Microsoft.Web.WebView2.Core;

namespace ReactDesktop;

public partial class MainWindow
{
    private bool _isUiReady;
    private ApiState _state;
    
    private async Task InitializeApi()
    {
        // Just in case
        await WebView.EnsureCoreWebView2Async();

        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? jsonString = e.WebMessageAsJson;
        UiMessage? message = JsonSerializer.Deserialize<UiMessage>(jsonString);
        
        // Invalid 
        if (message is null)
        {
            return;
        }

        Dispatch(message);
    }

    private void Dispatch(UiMessage message)
    {
        switch (message.Type)
        {
            case MessageTypes.UiReady:
                _isUiReady = true;
                break;
            case MessageTypes.GetConnectionString:
                
                break;
            case MessageTypes.SetConnectionString:
                break;
            case MessageTypes.GetLogLines:
                break;
            case MessageTypes.WriteLogLine:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
        }
    }
}
