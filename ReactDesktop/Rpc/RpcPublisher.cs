using System.Text.Json;

namespace ReactDesktop.Rpc;

public class RpcPublisher
{
    private Func<RpcEnvelope, Task> SendAsync { get; }

    public RpcPublisher(Func<RpcEnvelope, Task> sendAsync)
    {
        SendAsync = sendAsync;
    }

    public Task NotifyAsync(string method, JsonElement? payload = null) =>
        SendAsync(new RpcEnvelope(method, null, payload));

    public Task NotifyAsync<T>(string method, T payload) =>
        NotifyAsync(method, (JsonElement?)JsonSerializer.SerializeToElement(payload));
}
