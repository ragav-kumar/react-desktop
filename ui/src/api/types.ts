export type Method =
    | 'UiReady'
    | 'GetConnectionString'
    | 'SetConnectionString'
    | 'GetLogLines'
    | 'WriteLogLine'
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

export type RpcResponse<T> = {
    Id?: string | null;
    Result?: null;
    Error: RpcError;
} | {
    Id?: string | null;
    Result: T;
    Error?: null;
}