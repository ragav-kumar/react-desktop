import { v4 as createGuid } from 'uuid';
import type { Method, RpcRequest, RpcResponse } from './types';

declare global {
    interface Window {
        chrome?: {
            webview?: {
                addEventListener: (eventName: string, callback: (e: { data: RpcResponse<unknown> }) => void) => void;
                postMessage: (request: RpcRequest<unknown>) => void;
            }
        }
    }
}

interface Pending {
    resolve: (v: unknown) => void;
    reject: (err: unknown) => void;
    timer: number | undefined;
}

interface RpcClient {
    call<TRequest, TResponse>(method: Method, args?: TRequest): Promise<TResponse>;
}

const timeoutMilliseconds = 5000;

const createRpcClient = (): RpcClient => {
    const pending: Record<string, Pending> = {};
    let initialized: boolean = false;

    const init = () => {
        if (initialized) {
            return;
        }
        initialized = true;

        window?.chrome?.webview?.addEventListener('message', ({ data }) => {
            if (!data.Id) {
                return;
            }

            const p = pending[data.Id];
            if (!p) {
                return;
            }

            delete pending[data.Id];
            if (p.timer != null) {
                clearTimeout(p.timer);
            }

            if (data.Result != null) {
                p.resolve(data.Result);
            } else {
                const rpcError = data.Error!;
                p.reject(new Error(`Error ${rpcError.Code}: ${rpcError.Message}`));
            }
        });
    };

    const call = async <TRequest, TResponse>(method: Method, args?: TRequest): Promise<TResponse> => {
        init();

        const id = createGuid();
        const request: RpcRequest<TRequest | undefined> = {
            Id: id,
            Method: method,
            Params: args,
        };

        return new Promise<TResponse>((resolve, reject) => {
            const timer = setTimeout(async () => {
                delete pending[id];
                reject(new Error(`RPC timeout`));
            }, timeoutMilliseconds);

            pending[id] = { resolve: resolve as never, reject, timer };

            window?.chrome?.webview?.postMessage(request);
        });
    };

    return { call };
};

export const rpcClient = createRpcClient();