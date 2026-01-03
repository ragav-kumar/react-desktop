using System.Text.Json;

namespace ReactDesktop.Rpc;

public sealed record RpcMethod(
    Type ParamsType,
    Type ResultType,
    Func<JsonElement?, CancellationToken, Task<object?>> Invoke
);
