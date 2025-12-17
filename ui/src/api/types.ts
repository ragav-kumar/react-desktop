export type RequestEndpoint =
    | 'UiReady'
    | 'GetConnectionString'
    | 'SetConnectionString'
    | 'GetLogLines'
    | 'WriteLogLine'
;

export interface RequestMessage<T> {
    Type: RequestEndpoint;
    RequestId: string;
    Payload: T;
}

export interface ResponseMessage<T> {
    RequestId: string;
    Payload: T;
}