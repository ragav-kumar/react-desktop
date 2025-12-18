using System.Text.Json;

namespace ReactDesktop.Rpc;

/// <summary>
/// Request structure per JSON-RPC spec
/// </summary>
/// <param name="Id">If null, this is a Notification, and the client does not care about the response.</param>
/// <param name="Method">A defined RPC method</param>
/// <param name="Params">Optional</param>
public sealed record RpcRequest(
    Guid? Id,
    string Method,
    JsonElement? Params
);
