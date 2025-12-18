namespace ReactDesktop.Rpc;

public class MethodRegistry
{
    private readonly Dictionary<string, RpcMethod> _dict = new();

    public RpcMethod this[string name]
    {
        get => _dict[name];
        set => _dict[name] = value;
    }
    
    public void Add<TParams, TResult>(string name, Func<TParams, CancellationToken, Task<TResult>> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(TParams),
            typeof(TResult),
            async (o, token) => await handler((TParams)o!, token)
        );
    }
    
    public void Add<TResult>(string name, Func<CancellationToken, Task<TResult>> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(void),
            typeof(TResult),
            async (o, token) => await handler(token)
        );
    }

    public void AddNotification<TParams>(string name, Action<TParams, CancellationToken> handler)
    {
        _dict[name] = new RpcMethod(
            typeof(TParams),
            typeof(void),
            (o, token) =>
            {
                handler((TParams)o!, token);
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
