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
            if (rpcMethodAttribute is null && rpcNotificationAttribute is null)
            {
                continue;
            }

            if (rpcMethodAttribute is not null && rpcNotificationAttribute is not null)
            {
                throw new InvalidOperationException("A method cannot be both a notification and a request.");
            }
            
            bool isNotification = rpcNotificationAttribute is not null;
            string rpcName = method.Name;
            _dict[rpcName] = ToRpcMethod(target, method, isNotification);
        }
    }

    private RpcMethod ToRpcMethod(object target, MethodInfo methodInfo, bool isNotification)
    {
        MethodMetaData methodMeta = new(methodInfo);

        // Ensure we've got a valid signature.
        if (isNotification)
        {
            // Validate that we don't return data.
            if (methodMeta.ReturnsGenericTask)
            {
                throw new InvalidOperationException($"Notification method '{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}' cannot return Task<T>.");
            }
        }
        else
        {
            // Validate that we return data.
            if (!methodMeta.ReturnsGenericTask)
            {
                throw new InvalidOperationException($"Request method '{methodInfo.DeclaringType?.FullName}.{methodInfo.Name}' must return Task<T>.");
            }
        }

        Func<JsonElement?, CancellationToken, Task<object?>> invoker = methodMeta.HasParams
            ? BuildInvokerWithParams(target, methodMeta)
            : BuildInvokerWithoutParams(target, methodMeta);
        
        return methodMeta.ToRpcMethod(invoker);
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
