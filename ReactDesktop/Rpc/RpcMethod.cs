namespace ReactDesktop.Rpc;

public sealed record RpcMethod(
    Type ParamsType,
    Type ResultType,
    Func<object?, CancellationToken, Task<object?>> Invoke
);
