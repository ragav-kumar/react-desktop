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
    private MethodRegistry _methods;
    
    private async Task InitializeApi()
    {
        // Just in case
        await WebView.EnsureCoreWebView2Async();
        
        RegisterMethods();

        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
    }

    private void RegisterMethods()
    {
        _methods = new MethodRegistry();
        
        _methods.AddNotification(Methods.UiReady, UiReady);
        _methods.Add(Methods.GetConnectionString, GetConnectionString);
        _methods.AddNotification<string>(Methods.SetConnectionString, SetConnectionString);
        _methods.Add<GetLogLinesParamsDto, string[]>(Methods.GetLogLines, GetLogLines);
        _methods.AddNotification<string>(Methods.WriteLogLine, WriteLogLine);
    }

    private void UiReady(CancellationToken _) =>
        _isUiReady = true;

    private Task<string?> GetConnectionString(CancellationToken _) =>
        Task.FromResult(_state.ConnectionString);
    
    private void SetConnectionString(string connectionString, CancellationToken _) =>
        _state.ConnectionString = connectionString;

    private Task<string[]> GetLogLines(GetLogLinesParamsDto dto, CancellationToken _) =>
        Task.FromResult(BusinessLogic.ReadLogLines(dto.Skip, dto.Take));

    private void WriteLogLine(string message, CancellationToken _) =>
        BusinessLogic.WriteLine(message);

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
            await PostResponse(new RpcResponse(
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
            await PostResponse(new RpcResponse(request.Id, jsonResult));
        }
        catch (OperationCanceledException)
        {
            await PostResponse(new RpcResponse(request.Id, new RpcError(ErrorCodes.Cancelled, "Request cancelled.")));
        }
        catch (Exception ex)
        {
            await PostResponse(new RpcResponse(request.Id, new RpcError(ErrorCodes.InternalError, ex.Message)));
        }
    }

    private Task PostResponse(RpcResponse response)
    {
        string json = JsonSerializer.Serialize(response);

        return Dispatcher.InvokeAsync(() =>
        {
            WebView.CoreWebView2.PostWebMessageAsJson(json);
        }).Task;
    }
}
