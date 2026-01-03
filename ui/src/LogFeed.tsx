import { Api } from './api';

export const LogFeed = () => {
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
                Log feed goes here
            </pre>
        </div>
    );
};