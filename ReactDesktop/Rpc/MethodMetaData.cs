using System.Reflection;
using System.Text.Json;

namespace ReactDesktop.Rpc;

public class MethodMetaData
{
    public MethodInfo Method { get; }
    public Type ParamsType { get; private set; }
    public Type ResultType { get; private set; }
    public bool ReturnsTask { get; private set; }
    public bool ReturnsGenericTask { get; private set; }

    public bool HasParams => ParamsType != typeof(void);
    
    public MethodMetaData(MethodInfo method)
    {
        Method = method;
        ExtractParamsType();
        ExtractResultType();
    }

    public RpcMethod ToRpcMethod(Func<JsonElement?, CancellationToken, Task<object?>> invoke) => new(ParamsType, ResultType, invoke);
    
    private void ExtractParamsType()
    {
        ParameterInfo[] methodParams = Method.GetParameters();
        
        switch (methodParams.Length)
        {
            // Params: (CancellationToken)
            case 1 when methodParams[0].ParameterType == typeof(CancellationToken):
                ParamsType = typeof(void);
                return;
            // Params: (TParams, CancellationToken)
            case 2 when methodParams[1].ParameterType == typeof(CancellationToken):
                ParamsType = methodParams[0].ParameterType;
                return;
            default:
                throw new InvalidOperationException($"RPC handler '{Method.DeclaringType?.FullName}.{Method.Name}' must be (CancellationToken) or (TParams, CancellationToken).");
        }
    }
    
    private void ExtractResultType()
    {
        Type returnType = Method.ReturnType;

        // Notification
        if (returnType == typeof(void))
        {
            ResultType = typeof(void);
            ReturnsTask = false;
            ReturnsGenericTask = false;
            return;
        }
        
        // Async notification
        if (returnType == typeof(Task))
        {
            ResultType = typeof(void);
            ReturnsTask = true;
            ReturnsGenericTask = false;
            return;
        }

        // Request (always async)
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            ResultType = returnType.GetGenericArguments()[0];
            ReturnsTask = true;
            ReturnsGenericTask = true;
            return;
        }

        throw new InvalidOperationException(
            $"RPC handler '{Method.DeclaringType?.FullName}.{Method.Name}' must return void, Task, or Task<T>.");
    }
}
