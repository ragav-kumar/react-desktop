using System.Text.Json;

namespace ReactDesktop.Rpc;

/// <summary>
/// Response structure per JSON-RPC spec
/// </summary>
/// <typeparam name="T">Result type, determined by corresponding request</typeparam>
public sealed record RpcResponse
{
    /// <summary>Must be the id of a received request, or null if failed to parse the id on a request.</summary>
    public Guid? Id { get; }

    /// <summary>If set, Error must be null.</summary>
    public JsonElement? Result { get; }

    /// <summary>If set, Result must be null.</summary>
    public RpcError? Error { get; }

    public RpcResponse(Guid? id, JsonElement? result)
    {
        Id = id;
        Result = result;
        Error = null;
    }

    public RpcResponse(Guid? id, RpcError? error)
    {
        Id = id;
        Result = null;
        Error = error;
    }
}
