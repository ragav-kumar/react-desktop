using System.Reflection;
using System.Text.Json;

namespace ReactDesktop.Rpc;

public class MethodMetaData
{
    public MethodInfo Method { get; }
    public Type ParamsType { get; }
    public Type ResultType { get; }
    public bool ReturnsTask { get; private set; }
    public bool ReturnsGenericTask { get; private set; }

    public bool HasParams => ParamsType != typeof(void);
    
    public MethodMetaData(MethodInfo method)
    {
        Method = method;
        ParamsType = ExtractParamsType();
        ResultType = ExtractResultType();
    }

    public RpcMethod ToRpcMethod(Func<JsonElement?, CancellationToken, Task<object?>> invoke) => new(ParamsType, ResultType, invoke);
    
    private Type ExtractParamsType()
    {
        ParameterInfo[] methodParams = Method.GetParameters();

        return methodParams.Length switch
        {
            // Params: (CancellationToken)
            1 when methodParams[0].ParameterType == typeof(CancellationToken) => typeof(void),
            // Params: (TParams, CancellationToken)
            2 when methodParams[1].ParameterType == typeof(CancellationToken) => methodParams[0].ParameterType,
            _ => throw new InvalidOperationException($"RPC handler '{Method.DeclaringType?.FullName}.{Method.Name}' must be (CancellationToken) or (TParams, CancellationToken).")
        };
    }
    
    private Type ExtractResultType()
    {
        Type returnType = Method.ReturnType;

        // Notification
        if (returnType == typeof(void))
        {
            ReturnsTask = false;
            ReturnsGenericTask = false;
            return typeof(void);
        }
        
        // Async notification
        if (returnType == typeof(Task))
        {
            ReturnsTask = true;
            ReturnsGenericTask = false;
            return typeof(void);
        }

        // Request (always async)
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            ReturnsTask = true;
            ReturnsGenericTask = true;
            return returnType.GetGenericArguments()[0];
        }

        throw new InvalidOperationException(
            $"RPC handler '{Method.DeclaringType?.FullName}.{Method.Name}' must return void, Task, or Task<T>.");
    }
}
