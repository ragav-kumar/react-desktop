import { Api } from './api';
import { useEffect, useState } from 'react';
import { rpcClient } from './api/rpcClient';

export const LogFeed = () => {
    const [logLines, setLogLines] = useState<string[]>([]);

    const startListening = async () => {
        console.log('1');
        await Api.StartListeningForLogLines();
        console.log('2');
        setLogLines(await Api.GetLogLines(0, -1));
        console.log('3');
    }

    const stopListening = async () => {
        await Api.StopListeningForLogLines();
    }

    useEffect(() => {
        Api.GetLogLines(0, -1)
            .then(setLogLines);

        const unsubscribe = rpcClient.onPush<string>('LogLinesPushNotification', message => {
            if (message.Result != null) {
                setLogLines(l => [ ...l, message.Result ]);
            }
        });

        return () => {
            unsubscribe();
        };
    }, []);

    return (
        <div>
            <div className='buttonBar'>
                <button onClick={startListening}>
                    Start listening for log changes
                </button>
                <button onClick={stopListening}>
                    Stop listening to log changes
                </button>
            </div>
            <pre>
                {logLines.map((line, i) => (
                    <div key={i}>
                        {line}
                    </div>
                ))}
            </pre>
        </div>
    );
};