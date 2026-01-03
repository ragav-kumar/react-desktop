import type { Method, RpcError } from './types';
import { useEffect, useState } from 'react';
import { rpcClient } from './rpcClient';

export const usePushNotifications = <TPayload>(
    method: Method,
    initialValue: TPayload,
) => {
    const [value, setValue] = useState<TPayload>(initialValue);
    const [error, setError] = useState<RpcError | null>(null);

    useEffect(() => {
        const unsubscribe = rpcClient.onPush<TPayload>(method, message => {
            if (message.Error) {
                setError(message.Error);
            } else {
                setError(null);
                setValue(message.Result);
            }
        });

        return () => {
            unsubscribe();
        };
    }, [method]);

    return {value, error};
};