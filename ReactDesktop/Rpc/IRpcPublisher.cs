using System.Text.Json;

namespace ReactDesktop.Rpc;

public interface IRpcPublisher
{
    Task NotifyAsync(string method, JsonElement? payload = null);
    Task NotifyAsync<T>(string method, T payload);
}
