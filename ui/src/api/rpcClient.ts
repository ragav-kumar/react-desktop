import { v4 as createGuid } from 'uuid';
import type { Method, RpcRequest, RpcEnvelope } from './types';

declare global {
    interface Window {
        chrome?: {
            webview?: {
                addEventListener: (eventName: string, callback: (e: { data: RpcEnvelope<unknown> }) => void) => void;
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

type Unsubscribe = () => void;
type PushHandler<TPayload> = (message: RpcEnvelope<TPayload>) => void;

interface RpcClient {
    /** For call-response semantics */
    call<TRequest, TResponse>(method: Method, args?: TRequest): Promise<TResponse>;
    /** For push notifications. Returns an unsubscribe function. */
    onPush<TPayload>(method: Method, handler: PushHandler<TPayload>): Unsubscribe;
}

const timeoutMilliseconds = 5000;

const createRpcClient = (): RpcClient => {
    const pending: Record<string, Pending> = Object.create(null);
    const pushHandlers: Partial<Record<Method, Set<PushHandler<unknown>>>> = Object.create(null);
    let initialized: boolean = false;

    const init = () => {
        if (initialized) {
            return;
        }
        initialized = true;

        window?.chrome?.webview?.addEventListener('message', ({ data }) => {
            const handleCorrelatedResponse = (id: string) => {
                const p = pending[id];
                if (!p) {
                    return;
                }

                delete pending[id];
                if (p.timer != null) {
                    clearTimeout(p.timer);
                }

                if (data.Result != null) {
                    p.resolve(data.Result);
                } else {
                    const rpcError = data.Error!;
                    p.reject(new Error(`Error ${rpcError.Code}: ${rpcError.Message}`));
                }
            };

            const handlePushNotification = () => {
                const handlers = pushHandlers[data.Method];
                if (handlers == null || handlers.size === 0) {
                    return;
                }
                for (const handler of handlers) {
                    handler(data);
                }
            };

            if (data.Id != null) {
                handleCorrelatedResponse(data.Id);
            } else {
                handlePushNotification();
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

    const onPush = <TPayload>(method: Method, handler: PushHandler<TPayload>): Unsubscribe => {
        init();

        if (pushHandlers[method] == null) {
            pushHandlers[method] = new Set();
        }

        const wrapped = handler as PushHandler<unknown>;
        pushHandlers[method].add(wrapped);

        // Return a cleanup function.
        return () => {
            const set = pushHandlers[method];
            if (set == null) {
                return;
            }
            set.delete(wrapped);
            if (set.size === 0) {
                delete pushHandlers[method];
            }
        };
    };

    return { call, onPush };
};

export const rpcClient = createRpcClient();