using System.Text.Json;

namespace ReactDesktop.Rpc;

public class MethodRegistry
{
    private readonly Dictionary<string, RpcMethod> _dict = new();

    public RpcMethod this[string name]
    {
        get => _dict[name];
        set => _dict[name] = value;
    }

    private static T DeserializeParams<T>(JsonElement? jsonParams)
    {
        if (jsonParams is null)
            throw new JsonException("Add() with params must have non-null params.");
        
        return jsonParams.Value.Deserialize<T>()
            ?? throw new JsonException("Failed to deserialize params.");
    }
    
    public void Add<TParams, TResult>(string name, Func<TParams, CancellationToken, Task<TResult>> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(TParams),
            typeof(TResult),
            async (jsonParams, token) => await handler(DeserializeParams<TParams>(jsonParams), token));
    }
    
    public void Add<TResult>(string name, Func<CancellationToken, Task<TResult>> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(void),
            typeof(TResult),
            async (_, token) => await handler(token)
        );
    }

    public void AddNotification<TParams>(string name, Action<TParams, CancellationToken> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(TParams),
            typeof(void),
            (jsonParams, token) =>
            {
                handler(DeserializeParams<TParams>(jsonParams), token);
                return Task.FromResult<object?>(null);
            }
        );
    }

    public void AddNotification(string name, Action<CancellationToken> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(void),
            typeof(void),
            (_, token) =>
            {
                handler(token);
                return Task.FromResult<object?>(null);
            }
        );
    }

    public bool TryGet(string name, out RpcMethod? entry) => _dict.TryGetValue(name, out entry);

    public RpcMethod Get(string name) => _dict[name];
}
