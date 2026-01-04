using System.CodeDom;
using System.Reflection;
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

    // Empty constructor because I want to be able to loop registration across multiple classes if needed.
    public MethodRegistry()
    {
    }

    public MethodRegistry(object target)
    {
        RegisterMethods(target);
    }

    private static object? DeserializeParams(JsonElement? jsonParams, Type paramsType)
    {
        if (paramsType == typeof(void))
        {
            return null;
        }
        
        if (jsonParams is null)
        {
            throw new JsonException("Add() with params must have non-null params.");
        }
        
        return jsonParams.Value.Deserialize(paramsType)
            ?? throw new JsonException("Failed to deserialize params.");
    }

    public bool TryGet(string name, out RpcMethod? entry) => _dict.TryGetValue(name, out entry);

    public RpcMethod Get(string name) => _dict[name];

    private const BindingFlags AttributeFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    public void RegisterMethods(object target)
    {
        Type targetType = target.GetType();
        foreach (MethodInfo method in targetType.GetMethods(AttributeFlags))
        {
            if (method.IsAbstract || method.IsGenericMethodDefinition)
            {
                continue;
            }
            
            RpcRequestAttribute? rpcMethodAttribute = method.GetCustomAttribute<RpcRequestAttribute>(inherit: true);
            RpcNotificationAttribute? rpcNotificationAttribute = method.GetCustomAttribute<RpcNotificationAttribute>(inherit: true);
            RpcPushAttribute? rpcPushAttribute = method.GetCustomAttribute<RpcPushAttribute>(inherit: true);

            List<object?> attributes = new object?[]{rpcPushAttribute, rpcNotificationAttribute, rpcMethodAttribute}.Where(a => a is not null).ToList();
            int count = attributes.Count;
            if (count == 0)
            {
                continue;
            }
            if (count > 1)
            {
                throw new InvalidOperationException($"Method '{method.DeclaringType?.FullName}.{method.Name}' has multiple RPC attributes.");
            }
            
            _dict[method.Name] = attributes.Single() switch
            {
                RpcRequestAttribute      => ToRpcRequestMethod(target, method),
                RpcNotificationAttribute => ToRpcNotificationMethod(target, method),
                RpcPushAttribute         => ToRpcPushMethod(target, method),
                _                        => throw new InvalidOperationException("This should not be reachable.")
            };
        }
    }

    private static RpcMethod ToRpcRequestMethod(object target, MethodInfo methodInfo)
    {
        MethodMetaData methodMeta = new(methodInfo);
        
        // Validate that we return data.
        if (!methodMeta.ReturnsGenericTask)
        {
            throw new InvalidOperationException($"Request method '{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}' must return Task<T>.");
        }

        Func<JsonElement?, CancellationToken, Task<object?>> invoker = methodMeta.HasParams
            ? BuildInvokerWithParams(target, methodMeta)
            : BuildInvokerWithoutParams(target, methodMeta);
        
        return methodMeta.ToRpcMethod(invoker);
    }

    private RpcMethod ToRpcNotificationMethod(object target, MethodInfo methodInfo)
    {
        MethodMetaData methodMeta = new(methodInfo);
        
        // Validate that we don't return data.
        if (methodMeta.ReturnsGenericTask)
        {
            throw new InvalidOperationException($"Notification method '{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}' cannot return Task<T>.");
        }

        Func<JsonElement?, CancellationToken, Task<object?>> invoker = methodMeta.HasParams
            ? BuildInvokerWithParams(target, methodMeta)
            : BuildInvokerWithoutParams(target, methodMeta);
        
        return methodMeta.ToRpcMethod(invoker);
    }

    private RpcMethod ToRpcPushMethod(object target, MethodInfo methodInfo)
    {
        // Fixed signature: (IRpcPublisher) => void
        // MethodMeta would need to be reworked for this one, so let's just do it manually.
        
        ParameterInfo[] parameters = methodInfo.GetParameters();

        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IRpcPublisher) || methodInfo.ReturnType != typeof(void))
        {
            throw new InvalidOperationException($"Push method '{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}' must have signature (IRpcPublisher) => void.");
        }

        Func<JsonElement?, CancellationToken, Task<object?>> invoker = async (_, _) =>
        {
            // Discard result, Push methods are fire-and-forget.
            object? _ = methodInfo.Invoke(target, [parameters[0]]);
            return null;
        };

        return new RpcMethod
        (
            parameters[0].ParameterType,
            typeof(void),
            (_, _) =>
            {
                // Discard result, Push methods are fire-and-forget.
                object? _ = methodInfo.Invoke(target, [parameters[0]]);
                // To satisfy signature
                return Task.FromResult<object?>(null);
            }
        );
    }

    private static Func<JsonElement?, CancellationToken, Task<object?>> BuildInvokerWithoutParams(object target, MethodMetaData methodMeta) =>
        async (_, token) =>
        {
            object? rawResult = methodMeta.Method.Invoke(target, [token]);

            return await ReturnInvokeResult(methodMeta, rawResult);
        };

    private static Func<JsonElement?, CancellationToken, Task<object?>> BuildInvokerWithParams(object target, MethodMetaData methodMeta) =>
        async (jsonParams, token) =>
        {
            object? arg = DeserializeParams(jsonParams, methodMeta.ParamsType);
            object? rawResult = methodMeta.Method.Invoke(target, [arg, token]);

            return await ReturnInvokeResult(methodMeta, rawResult);
        };

    private static async Task<object?> ReturnInvokeResult(MethodMetaData methodMeta, object? rawResult)
    {
        // synchronous notification, just bail out.
        if (!methodMeta.ReturnsTask || rawResult is null)
        {
            return null;
        }

        Task task = (Task)rawResult;
        await task.ConfigureAwait(false);

        // Async notification, nothing more needed.
        if (!methodMeta.ReturnsGenericTask)
        {
            return null;
        }
            
        // Request (always async), need to get task data.
        PropertyInfo? resultProp = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
        return resultProp!.GetValue(task);
    }
}
