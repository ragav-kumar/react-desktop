using System.Text.Json;

namespace ReactDesktop.Rpc;

/// <summary>
/// Response structure per JSON-RPC spec
/// </summary>
public sealed record RpcEnvelope
{
    /// <summary>
    /// Mandatory, allows us to support push notifications. 
    /// </summary>
    public string Method { get; }

    /// <summary>Must be the id of a received request, or null if failed to parse the id on a request.</summary>
    public Guid? Id { get; }

    /// <summary>If set, Error must be null.</summary>
    public JsonElement? Result { get; }

    /// <summary>If set, Result must be null.</summary>
    public RpcError? Error { get; }

    public RpcEnvelope(string method, Guid? id, JsonElement? result)
    {
        Method = method;
        Id = id;
        Result = result;
        Error = null;
    }

    public RpcEnvelope(string method, Guid? id, RpcError? error)
    {
        Method = method;
        Id = id;
        Result = null;
        Error = error;
    }
}
