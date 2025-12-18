namespace ReactDesktop.Rpc;

public static class ErrorCodes
{
    // From the spec
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
    
    // All custom error codes must be outside the range of [-32768, -32000]
    // Add as needed.
    public const int Cancelled = -1;
}
