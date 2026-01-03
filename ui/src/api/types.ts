export type Method =
    | 'UiReady'
    | 'GetConnectionString'
    | 'SetConnectionString'
    | 'GetLogLines'
    | 'WriteLogLine'
    | 'StartListeningForLogLines'
    | 'StopListeningForLogLines'
    | 'LogLinesPushNotification'
;

export interface RpcRequest<T> {
    Id?: string | null;
    Method: Method;
    Params: T;
}

export interface RpcError {
    Code: number;
    Message: string;
    Data?: unknown;
}

/**
 * All server messages will include method information.
 * If an Id is present, this is a "correlated" message, following JSON RPC semantics.
 * If no Id is present, this is an "uncorrelated" message, basically a push notification.
 * For push notification, routing is done purely through the method.
 */
export type RpcEnvelope<T> =
    | { Method: Method; Id: string; Result: T;     Error?: null    }
    | { Method: Method; Id: string; Result?: null; Error: RpcError }
    | { Method: Method; Id?: null;  Result: T;     Error?: null    }
    | { Method: Method; Id?: null;  Result?: null; Error: RpcError };