using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using ReactDesktop.Rpc;

namespace ReactDesktop;

public partial class MainWindow
{
    /// <summary>
    /// Set once UI is initialized. Not used in this example, but might be useful if we need server side events.
    /// For example, progress bars or log feeds.
    /// </summary>
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool _isUiReady;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    private readonly ApiState _state;
    private readonly MethodRegistry _methods;

    // Push notification events (Only one in this example, have to make a separate one for each kind of push)
    private event Action<string?> LogLineWritten;
    
    private async Task InitializeApi()
    {
        // Just in case
        await WebView.EnsureCoreWebView2Async();

        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
    }

    [RpcNotification]
    private void UiReady(CancellationToken _) =>
        _isUiReady = true;

    [RpcRequest]
    private Task<string?> GetConnectionString(CancellationToken _) =>
        Task.FromResult(_state.ConnectionString);
    
    [RpcNotification]
    private void SetConnectionString(string connectionString, CancellationToken _)
    {
        _state.ConnectionString = connectionString;
        string message = LogFileApi.ReadAllLogLines().Last();
        LogLineWritten.Invoke(message);
    }

    [RpcRequest]
    private Task<string[]> GetLogLines(GetLogLinesParamsDto dto, CancellationToken _) =>
        Task.FromResult(LogFileApi.ReadLogLines(dto.Skip, dto.Take));

    [RpcNotification]
    private void WriteLogLine(string message, CancellationToken _)
    {
        LogFileApi.WriteLine(message);
        LogLineWritten.Invoke(message);
    }

    [RpcNotification]
    private void StartListeningForLogLines(CancellationToken _) =>
        _state.IsListeningForLogLineChanges = true;
    
    [RpcNotification]
    private void StopListeningForLogLines(CancellationToken _) =>
        _state.IsListeningForLogLineChanges = false;
    
    [RpcRequest]
    private Task<bool> IsListeningForLogLineChanges(CancellationToken _) =>
        Task.FromResult(_state.IsListeningForLogLineChanges);

    [RpcPush]
    private void LogLinesPushNotification(IRpcPublisher publisher)
    {
        LogLineWritten += message =>
        {
            if (!_state.IsListeningForLogLineChanges)
            {
                return;
            }
            
            _ = publisher.NotifyAsync(nameof(LogLinesPushNotification), message);
        };
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? jsonString = e.WebMessageAsJson;
        RpcRequest? message = JsonSerializer.Deserialize<RpcRequest>(jsonString);
        
        // Invalid 
        if (message is null)
        {
            return;
        }

        _ = Dispatch(message);
    }

    private async Task Dispatch(RpcRequest request)
    {
        if (!_methods.TryGet(request.Method, out RpcMethod? method) || method is null)
        {
            await PostResponse(new RpcEnvelope(
                method: request.Method,
                id: request.Id,
                error: new RpcError(ErrorCodes.MethodNotFound, $"Method '{request.Method}' not found.")
            ));

            return;
        }

        try
        {
            using CancellationTokenSource cts = new();

            object? result = await method.Invoke(request.Params, cts.Token);
            JsonElement? jsonResult = result is null
                ? null
                : JsonSerializer.SerializeToElement(result, result.GetType());
            await PostResponse(new RpcEnvelope(request.Method, request.Id, jsonResult));
        }
        catch (OperationCanceledException)
        {
            await PostResponse(new RpcEnvelope(request.Method, request.Id, new RpcError(ErrorCodes.Cancelled, "Request cancelled.")));
        }
        catch (Exception ex)
        {
            await PostResponse(new RpcEnvelope(request.Method, request.Id, new RpcError(ErrorCodes.InternalError, ex.Message)));
        }
    }

    private Task PostResponse(RpcEnvelope response)
    {
        string json = JsonSerializer.Serialize(response);

        return Dispatcher.InvokeAsync(() =>
        {
            WebView.CoreWebView2.PostWebMessageAsJson(json);
        }).Task;
    }
}
