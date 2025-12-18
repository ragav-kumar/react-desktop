using System.Text.Json;

namespace ReactDesktop.Rpc;

public record RpcError(int Code, string Message, JsonElement? Data = null);
