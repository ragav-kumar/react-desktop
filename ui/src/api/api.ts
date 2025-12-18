import { rpcClient } from './rpcClient';
import type { GetLogLinesParamsDto } from './dtos';

export const Api = {
    UiReady: (): Promise<void> => rpcClient.call('UiReady'),

    GetConnectionString: (): Promise<string> => rpcClient.call('GetConnectionString'),
    SetConnectionString: (connectionString: string): Promise<void> => rpcClient.call('SetConnectionString', connectionString),

    GetLogLines: (skip: number, take: number): Promise<string[]> =>
        rpcClient.call<GetLogLinesParamsDto, string[]>(
            'GetLogLines',
            { Skip: skip, Take: take }
        ),
    WriteLogLine: (logLine: string): Promise<void> => rpcClient.call('WriteLogLine', logLine),
} as const;