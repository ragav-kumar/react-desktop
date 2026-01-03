import { rpcClient } from './rpcClient';
import type { GetLogLinesParamsDto } from './dtos';
import type { Method } from './types';

export const Api = {
    UiReady: (): Promise<void> => callWithErrorHandling('UiReady'),

    GetConnectionString: (): Promise<string> => callWithErrorHandling('GetConnectionString'),
    SetConnectionString: (connectionString: string): Promise<void> => callWithErrorHandling('SetConnectionString', connectionString),

    GetLogLines: (skip: number, take: number): Promise<string[]> =>
        callWithErrorHandling<GetLogLinesParamsDto, string[]>(
            'GetLogLines',
            { Skip: skip, Take: take }
        ),
    WriteLogLine: (logLine: string): Promise<void> => callWithErrorHandling('WriteLogLine', logLine),
    StartListeningForLogLines: (): Promise<void> => callWithErrorHandling('StartListeningForLogLines'),
    StopListeningForLogLines: (): Promise<void> => callWithErrorHandling('StopListeningForLogLines'),
} as const;

const callWithErrorHandling = async <TRequest, TResponse>(method: Method, args?: TRequest): Promise<TResponse> => {
    try {
        return await rpcClient.call(method, args);
    } catch (e) {
        alert(e);
        throw e;
    }
};