import { Api } from './api';
import { usePushNotifications } from './api/usePushNotifications';

export const LogFeed = () => {
    const { value } = usePushNotifications<string[]>('LogLinesPushNotification', []);

    return (
        <div>
            <div>
                <button onClick={Api.StartListeningForLogLines}>
                    Start listening for log changes
                </button>
                <button onClick={Api.StopListeningForLogLines}>
                    Stop listening to log changes
                </button>
                <button>
                    Write a log line
                </button>
            </div>
            <pre>
                {value}
            </pre>
        </div>
    );
};